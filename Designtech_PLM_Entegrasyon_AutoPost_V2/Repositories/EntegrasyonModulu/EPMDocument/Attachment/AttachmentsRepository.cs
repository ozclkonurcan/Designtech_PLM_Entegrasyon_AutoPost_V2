using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EmailSettings;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EPMDocument.Attachment;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.WindchillApiSettings;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel.CADDocumentMgmt;
using Designtech_PLM_Entegrasyon_AutoPost_V2.ViewModel.WTDocAttachmentsModel;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using IronPdf;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PdfiumViewer;
using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.EPMDocument.Attachment
{
	public class AttachmentsRepository : IAttachmentsService
	{
		private readonly IConfiguration _configuration;
		private readonly IEmailService _emailService;
		private readonly IGetWindchillApiServices  _getWindchillApiServices;

		public AttachmentsRepository(IEmailService emailService, IGetWindchillApiServices getWindchillApiServices)
		{
			_emailService = emailService;
			_getWindchillApiServices = getWindchillApiServices;
		}

		public async Task GetAttachments(string state, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword, string sourceApi, string endPoint, int oldAlternateLinkCount, string sablonDataDurumu)
		{
			

				var SQL_Attachments = "";

			if(state == "SEND_FILE")
			{
				SQL_Attachments = $"SELECT [Ent_ID], [EPMDocID], [StateDegeri],[idA3masterReference] FROM {catalogValue}.Des_EPMDocument_LogTable WHERE [StateDegeri] = 'RELEASED'";
			}

			if (state == "CANCELLED")
			{
				SQL_Attachments = $"SELECT [Ent_ID], [EPMDocID], [StateDegeri],[idA3masterReference] FROM {catalogValue}.Des_EPMDocument_LogTable_Cancelled WHERE [StateDegeri] = 'CANCELLED'";
			}

				var responseData = await conn.QueryAsync<dynamic>(SQL_Attachments);

			var dataList = responseData.ToList();


			WTUsers globalUserEmail = new WTUsers();


				try
			{
				WindchillApiService windchillApiService = new WindchillApiService();
				foreach (var partItem in dataList)
				{

					var json = "";
					var cadJSON = "";
					var cadJSON2 = "";
					var jsonWTUSER = "";
					var cadReferencesJSON = "";

					WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);
					CSRF_NONCE = apiToken.NonceValue;

					cadJSON = await windchillApiService.GetApiData(WindchillServerName, $"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')?$expand=Attachments", BasicUsername, BasicPassword, CSRF_NONCE);
					cadJSON2 = await windchillApiService.GetApiData(WindchillServerName, $"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')?$expand=Representations", BasicUsername, BasicPassword, CSRF_NONCE);
					cadReferencesJSON = await windchillApiService.GetApiData(WindchillServerName, $"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')/References", BasicUsername, BasicPassword, CSRF_NONCE);
					jsonWTUSER = await windchillApiService.GetApiDataUser(WindchillServerName, $"PrincipalMgmt/Users?$select=EMail,Name,FullName", BasicUsername, BasicPassword, CSRF_NONCE);



					//cadJSON = await _getWindchillApiServices.GetApiData($"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')?$expand=Attachments");
					//cadJSON2 = await _getWindchillApiServices.GetApiData($"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')?$expand=Representations");
					//cadReferencesJSON = await _getWindchillApiServices.GetApiData($"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')/References");
					//jsonWTUSER = await _getWindchillApiServices.GetApiData($"PrincipalMgmt/Users?$select=EMail,Name,FullName");







					try
					{

						
							var responseCreator = JsonConvert.DeserializeObject<WTUsers>(jsonWTUSER);
							await _emailService.EmailControlString(responseCreator);
							//Zaman kontrolünü buraya ekleyebilriiz bellir bir saat aralığında diğerleri gibi burasıda tetik alır farklı tablo üzerinden

							//partItem.idA3masterReference
							var checkOutControlQuery = $"SELECT 1 FROM {catalogValue}.EPMDocument WHERE [idA3masterReference] = '{partItem.idA3masterReference}' AND [statecheckoutInfo] = 'wrk'";

							var checkOutControl = await conn.QueryAsync<dynamic>(checkOutControlQuery);

							if (checkOutControl == null || checkOutControl.Count() == 0)
							{


								#region Attachments


								if (partItem.StateDegeri == "RELEASED")
								{
									var cadAssociationsJSON = "";

									var CADResponse = JsonConvert.DeserializeObject<TeknikResim>(cadJSON);
									string partCode = "";
									if (CADResponse.Attachments != null && CADResponse.Attachments.Count > 0)
									{

										if (cadReferencesJSON != null)
										{
											try
											{
												var CADReferencesResponse = JsonConvert.DeserializeObject<CADDocumentReferences>(cadReferencesJSON);
												var CADReferencesResponse_ID = CADReferencesResponse.Value.Where(x => x.DepType.Display == "Drawing Reference").FirstOrDefault().ID.ToString();

												if (string.IsNullOrEmpty(CADReferencesResponse_ID))
												{
													LogService logService = new LogService(_configuration);
													var jsonData4 = JsonConvert.SerializeObject(CADResponse);
													logService.CreateJsonFileLogError(jsonData4, "Released işlemi gerçekleştirildi ama Drawing Reference bulunamadı");
												}
												else
												{
													string patternReferences = @"OR:wt\.epm\.structure\.EPMReferenceLink:(\d+)";
													Regex regexReferences = new Regex(patternReferences);
													Match matchReferences = regexReferences.Match(CADReferencesResponse_ID);

													var empReferenceLinkID = "";
													if (matchReferences.Success)
													{
														empReferenceLinkID = matchReferences.Groups[1].Value;
													}

													var SQL_EPMReferenceLink = $"SELECT * FROM {catalogValue}.EPMReferenceLink WHERE [idA2A2] = '{empReferenceLinkID}'";
													var resolvedItems_SQL_EPMReferenceLink = await conn.QueryFirstOrDefaultAsync<dynamic>(SQL_EPMReferenceLink);
													var SQL_EPMDocument = $@"
SELECT * 
FROM [{catalogValue}].EPMDocument 
WHERE [idA3masterReference] = '{resolvedItems_SQL_EPMReferenceLink.idA3B5}'
  AND latestiterationInfo = 1
  AND versionIdA2versionInfo = (
      SELECT MAX(versionIdA2versionInfo)
      FROM [{catalogValue}].EPMDocument
      WHERE [idA3MasterReference] = '{resolvedItems_SQL_EPMReferenceLink.idA3B5}'
  )
  AND versionLevelA2versionInfo = (
      SELECT MAX(versionLevelA2versionInfo)
      FROM [{catalogValue}].EPMDocument
      WHERE [idA3MasterReference] = '{resolvedItems_SQL_EPMReferenceLink.idA3B5}'
)";

													var resolvedItems_SQL_EPMDocument = await conn.QuerySingleAsync<dynamic>(SQL_EPMDocument);

													var SQL_EPMDocumentMaster = $@"
SELECT * 
FROM [{catalogValue}].EPMDocumentMaster 
WHERE [idA2A2] = '{resolvedItems_SQL_EPMDocument.idA3masterReference}'";

													var resolvedItems_SQL_EPMDocumentMaster = await conn.QuerySingleAsync<dynamic>(SQL_EPMDocumentMaster);


													if (resolvedItems_SQL_EPMDocument.statestate == "RELEASED")
													{



														var EPMBuildRuleSON = await windchillApiService.GetApiData(WindchillServerName, $"{sourceApi + resolvedItems_SQL_EPMDocument.idA2A2}')/PartDocAssociations", BasicUsername, BasicPassword, CSRF_NONCE);
														var EPMBuildRuleSONResponse = JsonConvert.DeserializeObject<CADDocumentResponse>(EPMBuildRuleSON);

														if (EPMBuildRuleSONResponse.Value.Count > 0)
														{
															if (EPMBuildRuleSONResponse.Value.Count > 0)
															{
																var EPMBuildRuleAssociations = EPMBuildRuleSONResponse.Value.FirstOrDefault().ID;
																string patternEPMBuildRule = @"OR:wt\.epm\.build\.EPMBuildRule:(\d+)";
																Regex regexEPMBuildRule = new Regex(patternEPMBuildRule);
																Match matchEPMBuildRule = regexEPMBuildRule.Match(EPMBuildRuleAssociations);
																var EPMBuildRuleID = "";
																if (matchEPMBuildRule.Success)
																{
																	EPMBuildRuleID = matchEPMBuildRule.Groups[1].Value;

																}


																var SQL_EPMBuildRule = $"SELECT * FROM {catalogValue}.EPMBuildRule WHERE [idA2A2] = '{EPMBuildRuleID}'";
																var resolvedItems_SQL_EPMBuildRule = await conn.QuerySingleAsync<dynamic>(SQL_EPMBuildRule);
																var SQL_WTPart = $"SELECT * FROM {catalogValue}.WTPart WHERE [branchIditerationInfo] = '{resolvedItems_SQL_EPMBuildRule.branchIdA3B5}' and latestiterationInfo = 1";
																var resolvedItems_SQL_WTPart = await conn.QuerySingleAsync<dynamic>(SQL_WTPart);
																#region WTPart Alternate Kontrol pdf içinde
																//var pdfAlternateControlJson = await windchillApiService.GetApiData(WindchillServerName, $"ProdMgmt/Parts('OR:wt.part.WTPart:{resolvedItems_SQL_WTPart.idA2A2}')?$expand=Alternates", BasicUsername, BasicPassword, CSRF_NONCE);
																//var pdfAlternateControlJsonResponse = JsonConvert.DeserializeObject<Part>(pdfAlternateControlJson);
																//if(pdfAlternateControlJsonResponse.Alternates.Count() > 0)
																//{

																//}
																#endregion

																//var SQL_WTPartMaster = $"SELECT * FROM {catalogValue}.WTPartMaster WHERE [branchIditerationInfo] = '{resolvedItems_SQL_WTPart.idA3masterReference}'";
																//var resolvedItems_SQL_WTPartMaster = await conn.QuerySingleAsync<dynamic>(SQL_WTPartMaster);

																partCode = Convert.ToString(resolvedItems_SQL_WTPart.idA2A2);
															}











															if (cadJSON != null || cadJSON != "")
															{
																if (CADResponse.Attachments != null && CADResponse.Attachments.Count > 0)
																{
																	if (CADResponse.Attachments.Any(x => x.Content.Label.Contains(CADResponse.Number + '_' + CADResponse.Version)))
																	{

																		//var selectedAttachment = CADResponse.Attachments.FirstOrDefault(a => a.Content != null && a.Content.Label != null && a.Content.Label.Contains(CADResponse.Number));
																		var selectedAttachment = CADResponse.Attachments.FirstOrDefault(a =>
																		a.Content != null &&
																		a.Content.Label != null &&
																		a.Content.Label.Contains(CADResponse.Number + '_' + CADResponse.Version) &&
																		a.Content.Label.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
																	);
																		if (selectedAttachment != null)
																		{
																			//var pdfSettings = CADResponse.Attachments.FirstOrDefault().Content;
																			if (selectedAttachment is not null)
																			{
																				var pdfUrl = selectedAttachment.Content.URL;
																				var pdfFileName = selectedAttachment.Content.Label;
																				var SQL_WTPartControl = $"SELECT * FROM {catalogValue}.Des_WTPart_LogTable WHERE [ParcaPartID] = {partCode}";
																				var responseDataWTPart = await conn.QueryAsync<dynamic>(SQL_WTPartControl);
																				if (responseDataWTPart.Count() == 0)
																				{
																					await SendPdfToCustomerAttachmentFunctionAsync(pdfUrl, pdfFileName, apiFullUrl, apiURL, endPoint, partItem.EPMDocID, catalogValue, conn, CADResponse, state, partCode);
																			}
																			}
																		}
																		else
																		{
																			LogService logService = new LogService(_configuration);

																			var jsonData4 = JsonConvert.SerializeObject(CADResponse);
																			//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
																			logService.CreateJsonFileLogError(jsonData4, $"Released işlemi gerçekleştirildi ama Attachment da {CADResponse.Number}_{CADResponse.Version}'ı içeren PDF bulunmadı.");

																			//logService.CreateJsonFileLog(cadJSON + "Attachment da Veri bulunamadı.");
																			continue;
																		}
																	}
																	else
																	{
																		LogService logService = new LogService(_configuration);

																		var jsonData4 = JsonConvert.SerializeObject(CADResponse);
																		//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
																		logService.CreateJsonFileLogError(jsonData4, $"Released işlemi gerçekleştirildi ama Attachment da {CADResponse.Number}'ı içeren Attachment bulunmadı.");

																		//logService.CreateJsonFileLog(cadJSON + "Attachment da Veri bulunamadı.");
																		continue;
																	}
																}
																else
																{
																	LogService logService = new LogService(_configuration);

																	var jsonData4 = JsonConvert.SerializeObject(CADResponse);
																	//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
																	logService.CreateJsonFileLogError(jsonData4, "Released işlemi gerçekleştirildi ama Attachment da Veri bulunamadı bu nedenle gönderilmedi.");

																	//logService.CreateJsonFileLog(cadJSON + "Attachment da Veri bulunamadı.");
																	continue;
																}
															}


														}
														else
														{
															LogService logService = new LogService(_configuration);
															var jsonData4 = JsonConvert.SerializeObject(CADResponse);
															//logService.CreateJsonFileLogError(jsonData4, "CADReferencesResponse nesnesi null. Hata detayı: " + ex.Message);
															logService.CreateJsonFileLogError(jsonData4, "Released işlemi gerçekleştirildi ama ilişkilendirilmiş bir WTPart parça bulunamadı. ");
														continue;
													}

													}
													else
													{
														LogService logService = new LogService(_configuration);

														var jsonData4 = JsonConvert.SerializeObject(CADResponse);
														//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
														logService.CreateJsonFileLogError(jsonData4, $"Released işlemi gerçekleştirildi ama CAD Döküman Released değil. CADName : {resolvedItems_SQL_EPMDocumentMaster.CADName} Name : {resolvedItems_SQL_EPMDocumentMaster.name} DocumentNumber : {resolvedItems_SQL_EPMDocumentMaster.documentNumber} State : {resolvedItems_SQL_EPMDocument.statestate}");
													continue;
												}


												}
											}

											catch (NullReferenceException ex)
											{
												LogService logService = new LogService(_configuration);
												var jsonData4 = JsonConvert.SerializeObject(CADResponse);
												//logService.CreateJsonFileLogError(jsonData4, "CADReferencesResponse nesnesi null. Hata detayı: " + ex.Message);
												logService.CreateJsonFileLogError(jsonData4, "CAD Döküman References çıktısı boş.İlişkilendirilmiş bir WTPart parça bulunamadı. Hata detayı: " + ex.Message);
											continue;
										}
											catch (Exception ex)
											{
												LogService logService = new LogService(_configuration);
												var jsonData4 = JsonConvert.SerializeObject(CADResponse);
												logService.CreateJsonFileLogError(jsonData4, "Beklenmedik bir hata oluştu. Hata detayı: " + ex.Message);
											continue;
										}
										}

										// If LastUpdateTimestamp has not changed, do nothing

									}
									else
									{
										LogService logService = new LogService(_configuration);

										var jsonData4 = JsonConvert.SerializeObject(CADResponse);
										//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
										logService.CreateJsonFileLogError(jsonData4, "Released işlemi gerçekleştirildi ama Attachment da Veri bulunamadı bu nedenle gönderilmedi.");

										//logService.CreateJsonFileLog(cadJSON + "Attachment da Veri bulunamadı.");
										continue;
									}

								}
							#endregion

							#region CAD Cancelled yeni

							if (partItem.StateDegeri == "CANCELLED")
							{


								var cadAssociationsJSON = "";
								var CADResponse = JsonConvert.DeserializeObject<RootObject>(cadJSON2);

								if (cadReferencesJSON != null)
								{
									try
									{


										var CADReferencesResponse = JsonConvert.DeserializeObject<CADDocumentReferences>(cadReferencesJSON);
										var CADReferencesResponse_ID = CADReferencesResponse.Value.Where(x => x.DepType.Display == "Drawing Reference").FirstOrDefault().ID.ToString();

										if (string.IsNullOrEmpty(CADReferencesResponse_ID))
										{
											LogService logService = new LogService(_configuration);
											var jsonData4 = JsonConvert.SerializeObject(CADResponse);
											logService.CreateJsonFileLogError(jsonData4, "Cancelled işlemi gerçekleştirildi ama Drawing Reference bulunamadı");
										}
										else
										{

											string patternReferences = @"OR:wt\.epm\.structure\.EPMReferenceLink:(\d+)";
											Regex regexReferences = new Regex(patternReferences);
											Match matchReferences = regexReferences.Match(CADReferencesResponse_ID);

											var empReferenceLinkID = "";
											if (matchReferences.Success)
											{
												empReferenceLinkID = matchReferences.Groups[1].Value;
											}


											var SQL_EPMReferenceLink = $"SELECT * FROM {catalogValue}.EPMReferenceLink WHERE [idA2A2] = '{empReferenceLinkID}'";
											var resolvedItems_SQL_EPMReferenceLink = await conn.QueryFirstOrDefaultAsync<dynamic>(SQL_EPMReferenceLink);
											var SQL_EPMDocument = $@"
							SELECT * 
							FROM [{catalogValue}].EPMDocument 
							WHERE [idA3masterReference] = '{resolvedItems_SQL_EPMReferenceLink.idA3B5}'
							  AND latestiterationInfo = 1
							  AND versionIdA2versionInfo = (
							      SELECT MAX(versionIdA2versionInfo)
							      FROM [{catalogValue}].EPMDocument
							      WHERE [idA3MasterReference] = '{resolvedItems_SQL_EPMReferenceLink.idA3B5}'
							  )
							  AND versionLevelA2versionInfo = (
							      SELECT MAX(versionLevelA2versionInfo)
							      FROM [{catalogValue}].EPMDocument
							      WHERE [idA3MasterReference] = '{resolvedItems_SQL_EPMReferenceLink.idA3B5}'
							)";

											var resolvedItems_SQL_EPMDocument = await conn.QuerySingleAsync<dynamic>(SQL_EPMDocument);

											var SQL_EPMDocumentMaster = $@"
							SELECT * 
							FROM [{catalogValue}].EPMDocumentMaster 
							WHERE [idA2A2] = '{resolvedItems_SQL_EPMDocument.idA3masterReference}'";

											var resolvedItems_SQL_EPMDocumentMaster = await conn.QuerySingleAsync<dynamic>(SQL_EPMDocumentMaster);






											var EPMBuildRuleSON = await windchillApiService.GetApiData(WindchillServerName, $"{sourceApi + resolvedItems_SQL_EPMDocument.idA2A2}')/PartDocAssociations", BasicUsername, BasicPassword, CSRF_NONCE);
											var EPMBuildRuleSONResponse = JsonConvert.DeserializeObject<CADDocumentResponse>(EPMBuildRuleSON);

											var EPMBuildRuleAssociations = EPMBuildRuleSONResponse.Value.FirstOrDefault().ID;
											string patternEPMBuildRule = @"OR:wt\.epm\.build\.EPMBuildRule:(\d+)";
											Regex regexEPMBuildRule = new Regex(patternEPMBuildRule);
											Match matchEPMBuildRule = regexEPMBuildRule.Match(EPMBuildRuleAssociations);
											var EPMBuildRuleID = "";

											if (matchEPMBuildRule.Success)
											{
												EPMBuildRuleID = matchEPMBuildRule.Groups[1].Value;

											}








											if (resolvedItems_SQL_EPMDocument.statestate == "CANCELLED")
											{



												try
												{

													var CADViewResponse = new TeknikResimCancel
													{
														Number = "TR_" + CADResponse.Number,
														Revizyon = CADResponse.Revision,

													};


													ApiService _apiService = new ApiService();



													//var jsonData3 = JsonConvert.SerializeObject(anaPart);
													var LogJsonData = JsonConvert.SerializeObject(CADViewResponse);
													dynamic dataResponse = null;

													dataResponse = await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, LogJsonData, LogJsonData);

													LogService logService = new LogService(_configuration);

													logService.CreateJsonFileLog(LogJsonData, "CAD Döküman iptal edildi." + dataResponse.message);

													await conn.ExecuteAsync($@"
							                                            DELETE FROM [{catalogValue}].[Des_EPMDocument_LogTable_Cancelled]
							                                            WHERE EPMDocID = @Ids", new { Ids = partItem.EPMDocID });





												}
												catch (Exception ex)
												{
													//Burasıda cancelled olan parçanın düştüğü hata yeri bunun error ayarınıda buradan yapcaz

													var CADViewResponseContentInfoCatch = new TeknikResim2ViewModel
													{
														Number = "TR_" + CADResponse.Number,
														Revizyon = CADResponse.Revision,
													};

													var LogJsonDataCatch = JsonConvert.SerializeObject(CADViewResponseContentInfoCatch);
													LogService logService = new LogService(_configuration);
													logService.CreateJsonFileLog(LogJsonDataCatch, "HATA ! " + ex.Message);


													var Ent_EPMDocStateModelResponse = await conn.QueryFirstAsync<Ent_EPMDocStateModel>(
											$"SELECT * FROM [{catalogValue}].[Des_EPMDocument_LogTable_Cancelled] WHERE [EPMDocID] = {partItem.EPMDocID}");

													var existingErrorRecord = await conn.QueryFirstOrDefaultAsync<Ent_EPMDocStateModel>(
											$"SELECT * FROM [{catalogValue}].[Des_EPMDocument_LogTable_Cancelled_Error] WHERE [EPMDocID] = @EPMDocID AND [idA3masterReference] = @idA3masterReference",
											new { EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID, idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference }
											);



													if (existingErrorRecord == null)
													{
														// Veri yoksa yeni bir kayıt ekle
														await conn.ExecuteAsync(
															$"INSERT INTO [{catalogValue}].[Des_EPMDocument_LogTable_Cancelled_Error] ([EPMDocID],[StateDegeri], [idA3masterReference], [CadName],[name], [docNumber]) VALUES (@EPMDocID,@StateDegeri, @idA3masterReference, @CadName,@name, @docNumber)",
															new
															{
																EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID,
																StateDegeri = Ent_EPMDocStateModelResponse.StateDegeri,
																idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference,
																CadName = Ent_EPMDocStateModelResponse.CadName,
																name = Ent_EPMDocStateModelResponse.CadName,
																docNumber = Ent_EPMDocStateModelResponse.docNumber
															}
														);
													}
													else
													{
														// Veri varsa güncelle
														await conn.ExecuteAsync(
															$"UPDATE [{catalogValue}].[Des_EPMDocument_LogTable_Cancelled_Error] SET [StateDegeri] = @StateDegeri, [CadName] = @CadName, [name] = @name, [docNumber] = @docNumber WHERE [EPMDocID] = @EPMDocID AND [idA3masterReference] = @idA3masterReference",
															new
															{
																EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID,
																StateDegeri = Ent_EPMDocStateModelResponse.StateDegeri,
																idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference,
																CadName = Ent_EPMDocStateModelResponse.CadName,
																name = Ent_EPMDocStateModelResponse.CadName,
																docNumber = Ent_EPMDocStateModelResponse.docNumber
															}
														);
													}




													await conn.ExecuteAsync($@"
							                                            DELETE FROM [{catalogValue}].[Des_EPMDocument_LogTable_Cancelled]
							                                            WHERE EPMDocID = @Ids", new { Ids = partItem.EPMDocID });


												}







											}
											else
											{
												LogService logService = new LogService(_configuration);

												var jsonData4 = JsonConvert.SerializeObject(CADResponse);
												//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
												logService.CreateJsonFileLogError(jsonData4, $"Cancelled işlemi gerçekleştirildi ama CAD Döküman Cancelled değil. CADName : {resolvedItems_SQL_EPMDocumentMaster.CADName} Name : {resolvedItems_SQL_EPMDocumentMaster.name} DocumentNumber : {resolvedItems_SQL_EPMDocumentMaster.documentNumber} State : {resolvedItems_SQL_EPMDocument.statestate}");
											}





										}
									}
									catch (NullReferenceException ex)
									{
										LogService logService = new LogService(_configuration);
										var jsonData4 = JsonConvert.SerializeObject(CADResponse);
										//logService.CreateJsonFileLogError(jsonData4, "CADReferencesResponse nesnesi null. Hata detayı: " + ex.Message);
										logService.CreateJsonFileLogError(jsonData4, "CAD Döküman References çıktısı boş.İlişkilendirilmiş bir WTPart parça bulunamadı. Hata detayı: " + ex.Message);
									}
									catch (Exception ex)
									{
										LogService logService = new LogService(_configuration);
										var jsonData4 = JsonConvert.SerializeObject(CADResponse);
										logService.CreateJsonFileLogError(jsonData4, "Beklenmedik bir hata oluştu. Hata detayı: " + ex.Message);
									}

								}



							}

							#endregion

						}










					}
					catch (Exception)
					{
						continue;
					}

				}



			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}


			
		}


		public async Task SendPdfToCustomerFunctionAsync(string pdfUrl, string pdfFileName, string apiFullUrl, string apiURL, string endPoint, long EPMDocID, string catalogValue, SqlConnection conn, RootObject CADResponse, string stateType, string partCode)
		{
			try
			{
				if (stateType == "SEND_FILE")
				{

					var partName = "";
					var partNumber = "";
					var partState = "";
					var projeCode = "";
					var json = "";
					if (!string.IsNullOrEmpty(partCode))
					{


						var SQL_WTPart = $"SELECT [idA3masterReference] ,[statestate] FROM {catalogValue}.WTPart WHERE [idA2A2] = '{partCode}'";
						var resolvedItems_SQL_WTPart = await conn.QuerySingleAsync<dynamic>(SQL_WTPart);
						var SQL_WTPartMaster = $"SELECT [name],[WTPartNumber] FROM {catalogValue}.WTPartMaster WHERE [idA2A2] = '{resolvedItems_SQL_WTPart.idA3masterReference}'";
						var resolvedItems_SQL_WTPartMaster = await conn.QuerySingleAsync<dynamic>(SQL_WTPartMaster);


						partName = resolvedItems_SQL_WTPartMaster.name;
						partNumber = resolvedItems_SQL_WTPartMaster.WTPartNumber;
						partState = resolvedItems_SQL_WTPart.statestate;
						json = await projectCodeRootObjectInfo(partCode, CADResponse);
					}


					try
					{
						var response = JsonConvert.DeserializeObject<PartPDF>(json);
						if (string.IsNullOrEmpty(response.ProjeKodu))
						{
							LogService logService = new LogService(_configuration);
							var jsonData4 = JsonConvert.SerializeObject(CADResponse);
							//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
							logService.CreateJsonFileLogError(jsonData4, $"WTPart'ın projectCode Attr. de değer bulunmuyor");
						}
						else
						{
							//if (resolvedItems_SQL_EPMBuildHistory != null || resolvedItems_SQL_EPMBuildHistory != "")
							//{
							//    partName = resolvedItems_SQL_WTPartMaster.name;
							//    partNumber = resolvedItems_SQL_WTPartMaster.WTPartNumber;
							//}
							//else
							//{
							//    partName = "Name değer bulunamadı";
							//    partNumber = "Number değeri bulunamadı";
							//}

							var CADViewResponseContentInfo = new TeknikResim2ViewModel
							{
								Number = "TR_" + CADResponse.Number,
								Revizyon = CADResponse.Revision,
								DocumentType = "TR",
								//Description = CADResponse.Description ?? "Null",
								Description = partName ?? "Null",
								ModifiedOn = CADResponse.LastModified,
								AuthorizationDate = CADResponse.LastModified,
								//ModifiedBy = CADResponse.ModifiedBy,
								ModifiedBy = "WindchillAD",
								state = 30,
								name = pdfFileName,
								content = await DownloadPdfAsync(pdfUrl),
								projectCode = response.ProjeKodu,
								relatedParts = new List<RelatedParts>
							{
								new RelatedParts
								{
									RelatedPartName = partName ?? "Null",
									RelatedPartNumber = partNumber ?? "Null",
									isUpdateAndDelete = false,
								}
							}


							};


							ApiService _apiService = new ApiService();



							//var jsonData3 = JsonConvert.SerializeObject(anaPart);
							var LogJsonData = JsonConvert.SerializeObject(CADViewResponseContentInfo);
							if (!string.IsNullOrEmpty(partCode))
							{

								if (partState == "RELEASED")
								{

									try
									{

										await WTDocumentAttachmentFunc(CADViewResponseContentInfo.content, CADViewResponseContentInfo.name, CADResponse.ID);



										dynamic dataResponse = null;

										// PDF iptal etmeden yada göndermeden önce wtpartı kontrol etmeliyiz.

										dataResponse = await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, LogJsonData, LogJsonData);


										string directoryPath = "Configuration";
										string fileName2 = "appsettings.json";
										string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName2);


										string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
										JObject jsonObject = JObject.Parse(jsonData);
										var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
										var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();
										var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();

										var ServerName = jsonObject["ServerName"].ToString();
										var Catalog = jsonObject["Catalog"].ToString();
										var DatabaseSchema = jsonObject["DatabaseSchema"].ToString();
										var KullaniciAdi = jsonObject["KullaniciAdi"].ToString();
										var Parola = jsonObject["Parola"].ToString();

										await conn.ExecuteAsync(
										   $"INSERT INTO [{catalogValue}].[Des_EPMDocAttachmentsLog] ([serverName],[databaseName],[databaseSchemaName],[databaseUsername],[databasePassword],[plmServerAdress],[plmUserName],[plmPassword], [epmFileName],[epmDocNumber],[epmFileContent]) VALUES (@serverName,@databaseName,@databaseSchemaName,@databaseUsername,@databasePassword,@plmServerAdress,@plmUserName,@plmPassword, @epmFileName, @epmDocNumber,@epmFileContent)",
										   new
										   {
											   serverName = ServerName,
											   databaseName = Catalog,
											   databaseSchemaName = DatabaseSchema,
											   databaseUsername = KullaniciAdi,
											   databasePassword = Parola,
											   plmServerAdress = WindchillServerName,
											   plmUserName = BasicUsername,
											   plmPassword = BasicPassword,
											   epmFileName = CADViewResponseContentInfo.name,
											   epmDocNumber = EPMDocID,
											   epmFileContent = CADViewResponseContentInfo.content,
										   }
									   );


										LogService logService = new LogService(_configuration);

										logService.CreateJsonFileLog(LogJsonData, "CAD Döküman bilgileri gönderildi." + dataResponse.message);

										//await SendPdfToCustomerApiAsync(pdfBytes, pdfFileName, customerApiEndpoint, CADViewResponseContentInfo);
										await conn.ExecuteAsync($@"
                    DELETE FROM [{catalogValue}].[Des_EPMDocument_LogTable]
                    WHERE EPMDocID = @Ids", new { Ids = EPMDocID });
										var now = DateTime.Now;
									




									}
									catch (Exception ex)
									{

										//Hata veren verityi ekleyen kısım burası diğleri gibi buraya kurulucak yapı 
										var CADViewResponseContentInfoCatch = new TeknikResim2ViewModel
										{
											Number = "TR_" + CADResponse.Number,
											Revizyon = CADResponse.Revision,
											DocumentType = "TR",
											//Description = CADResponse.Description,
											Description = partName,
											ModifiedOn = CADResponse.LastModified,
											AuthorizationDate = CADResponse.LastModified,
											//ModifiedBy = CADResponse.ModifiedBy,
											ModifiedBy = "WindchillAD",
											state = 30,
											name = pdfFileName,
											content = await DownloadPdfAsync(pdfUrl),
											projectCode = response.ProjeKodu,




										};
										var LogJsonDataCatch = JsonConvert.SerializeObject(CADViewResponseContentInfo);
										LogService logService = new LogService(_configuration);
										logService.CreateJsonFileLog(LogJsonData, "HATA ! " + ex.Message);

							



											var Ent_EPMDocStateModelResponse = await conn.QueryFirstAsync<Ent_EPMDocStateModel>(
					  $"SELECT * FROM [{catalogValue}].[Des_EPMDocument_LogTable] WHERE [EPMDocID] = {EPMDocID}");


											var existingErrorRecord = await conn.QueryFirstOrDefaultAsync<Ent_EPMDocStateModel>(
	$"SELECT * FROM [{catalogValue}].[Des_EPMDocument_LogTable_Error] WHERE [EPMDocID] = @EPMDocID AND [idA3masterReference] = @idA3masterReference",
	new { EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID, idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference }
	);



											if (existingErrorRecord == null)
											{
												// Veri yoksa yeni bir kayıt ekle
												await conn.ExecuteAsync(
													$"INSERT INTO [{catalogValue}].[Des_EPMDocument_LogTable_Error] ([EPMDocID],[StateDegeri], [idA3masterReference], [CadName],[name], [docNumber]) VALUES (@EPMDocID,@StateDegeri, @idA3masterReference, @CadName,@name, @docNumber)",
													new
													{
														EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID,
														StateDegeri = Ent_EPMDocStateModelResponse.StateDegeri,
														idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference,
														CadName = Ent_EPMDocStateModelResponse.CadName,
														name = Ent_EPMDocStateModelResponse.CadName,
														docNumber = Ent_EPMDocStateModelResponse.docNumber
													}
												);
											}
											else
											{
												// Veri varsa güncelle
												await conn.ExecuteAsync(
													$"UPDATE [{catalogValue}].[Des_EPMDocument_LogTable_Error] SET [StateDegeri] = @StateDegeri, [CadName] = @CadName, [name] = @name, [docNumber] = @docNumber WHERE [EPMDocID] = @EPMDocID AND [idA3masterReference] = @idA3masterReference",
													new
													{
														EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID,
														StateDegeri = Ent_EPMDocStateModelResponse.StateDegeri,
														idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference,
														CadName = Ent_EPMDocStateModelResponse.CadName,
														name = Ent_EPMDocStateModelResponse.CadName,
														docNumber = Ent_EPMDocStateModelResponse.docNumber
													}
												);
											}
										




										await conn.ExecuteAsync($@"
                                        DELETE FROM [{catalogValue}].[Des_EPMDocument_LogTable]
                                        WHERE EPMDocID = @Ids", new { Ids = EPMDocID });



									}




								}
								else
								{
									LogService logService = new LogService(_configuration);
									var jsonData4 = JsonConvert.SerializeObject(CADResponse);
									//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
									logService.CreateJsonFileLogError(jsonData4, $"Released işlemi gerçekleştirildi WTPart state durumu released değil. WTPart Name : {partName} - WTPart Number {partNumber} - WTPart State {partState}");
								}
							}
							else
							{
								LogService logService = new LogService(_configuration);
								var jsonData4 = JsonConvert.SerializeObject(CADResponse);
								//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
								logService.CreateJsonFileLogError(jsonData4, "Released işlemi gerçekleştirildi ama gönderilmedi. WTPart ilişkisi bulunmadı.");
							}
						}

					}
					catch (Exception ex)
					{
						LogService logService = new LogService(_configuration);
						var jsonData4 = JsonConvert.SerializeObject(CADResponse);
						logService.CreateJsonFileLogError(jsonData4, "HATA :" + ex.Message);
					}




				}


			}
			catch (Exception ex)
			{
				LogService logService = new LogService(_configuration);

				logService.CreateJsonFileLog(ex.Message, "HATA");

			}

		}


		public async Task SendPdfToCustomerAttachmentFunctionAsync(string pdfUrl, string pdfFileName, string apiFullUrl, string apiURL, string endPoint, long EPMDocID, string catalogValue, SqlConnection conn, TeknikResim CADResponse, string stateType, string partCode)
		{
			try
			{
				if (stateType == "SEND_FILE")
				{
					var partName = "";
					var partNumber = "";
					var partState = "";
					var projeCode = "";
					var idA3ViewName = "";
					var json = "";



					if (!string.IsNullOrEmpty(partCode))
					{


						var SQL_WTPart = $"SELECT [idA3masterReference] ,[statestate],[idA3View] FROM {catalogValue}.WTPart WHERE [idA2A2] = '{partCode}'";
						var resolvedItems_SQL_WTPart = await conn.QuerySingleAsync<dynamic>(SQL_WTPart);
						var SQL_WTPartMaster = $"SELECT [name],[WTPartNumber] FROM {catalogValue}.WTPartMaster WHERE [idA2A2] = '{resolvedItems_SQL_WTPart.idA3masterReference}'";
						var resolvedItems_SQL_WTPartMaster = await conn.QuerySingleAsync<dynamic>(SQL_WTPartMaster);


						partName = resolvedItems_SQL_WTPartMaster.name;
						partNumber = resolvedItems_SQL_WTPartMaster.WTPartNumber;
						partState = resolvedItems_SQL_WTPart.statestate;

						//Design Kontrolü yapılacak design değil ise pdf gönderme iptal ediliecek ve logdan da kaldırılacak tekrar denenmememsi için


						var SQL_WTPartIdA3View = $"SELECT [name] FROM {catalogValue}.WTView WHERE idA2A2 = '{resolvedItems_SQL_WTPart.idA3View}'";
						var resolvedItems_SQL_WTPartIdA3View = await conn.QueryFirstAsync<dynamic>(SQL_WTPartIdA3View);
						idA3ViewName = resolvedItems_SQL_WTPartIdA3View.name;
						//Design Kontrolü yapılacak design değil ise pdf gönderme iptal ediliecek ve logdan da kaldırılacak tekrar denenmememsi için


						//projeCode = resolvedItems_SQL_WTPart.ProjeKodu;

						//incelenecek1
						json = await projectCodeInfo(partCode, CADResponse); // partCode değerini fonksiyona gönderin
					}


					if (!string.IsNullOrEmpty(json)) // json boş değilse işle
					{
						var now = DateTime.Now;

						try
						{
							var response = JsonConvert.DeserializeObject<PartPDF>(json);
							if (string.IsNullOrEmpty(response.ProjeKodu))
							{
								LogService logService = new LogService(_configuration);
								var jsonData4 = JsonConvert.SerializeObject(CADResponse);
								//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
								logService.CreateJsonFileLogError(jsonData4, $"WTPart'ın projectCode Attr. de değer bulunmuyor");
							}
							else
							{

								dynamic CADViewResponseContentInfo = null;



								CADViewResponseContentInfo = new TeknikResim2ViewModel
								{
									Number = "TR_" + CADResponse.Number,
									Revizyon = CADResponse.Revision,
									DocumentType = "TR",
									//Description = CADResponse.Description ?? "Null",
									Description = partName ?? "Null",
									ModifiedOn = CADResponse.LastModified,
									AuthorizationDate = CADResponse.LastModified,
									//ModifiedBy = CADResponse.ModifiedBy,
									ModifiedBy = "WindchillAD",
									state = 30,
									name = pdfFileName,
									content = await DownloadPdfAsync(pdfUrl),
									projectCode = response.ProjeKodu,
									relatedParts = new List<RelatedParts>
							{
								new RelatedParts
								{
									RelatedPartName = partName ?? "Null",
									RelatedPartNumber = partNumber ?? "Null",
									isUpdateAndDelete = false,
								}
							}


								};



								ApiService _apiService = new ApiService();



								//var jsonData3 = JsonConvert.SerializeObject(anaPart);
								var LogJsonData = JsonConvert.SerializeObject(CADViewResponseContentInfo);
								if (!string.IsNullOrEmpty(partCode))
								{

									if(idA3ViewName == "Design")
									{


									if (partState == "RELEASED")
									{
										//var now = DateTime.Now;

										try
										{


											await WTDocumentAttachmentFunc(CADViewResponseContentInfo.content, CADViewResponseContentInfo.name, CADResponse.ID);




											dynamic dataResponse = null;

											dataResponse = await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, LogJsonData, LogJsonData);

											string directoryPath = "Configuration";
											string fileName2 = "appsettings.json";
											string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName2);


											string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
											JObject jsonObject = JObject.Parse(jsonData);
											var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
											var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();
											var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();

											var ServerName = jsonObject["ServerName"].ToString();
											var Catalog = jsonObject["Catalog"].ToString();
											var DatabaseSchema = jsonObject["DatabaseSchema"].ToString();
											var KullaniciAdi = jsonObject["KullaniciAdi"].ToString();
											var Parola = jsonObject["Parola"].ToString();


											await conn.ExecuteAsync(
											   $"INSERT INTO [{catalogValue}].[Des_EPMDocAttachmentsLog] ([serverName],[databaseName],[databaseSchemaName],[databaseUsername],[databasePassword],[plmServerAdress],[plmUserName],[plmPassword], [epmFileName],[epmDocNumber],[epmFileContent]) VALUES (@serverName,@databaseName,@databaseSchemaName,@databaseUsername,@databasePassword,@plmServerAdress,@plmUserName,@plmPassword, @epmFileName, @epmDocNumber,@epmFileContent)",
											   new
											   {
												   serverName = ServerName,
												   databaseName = Catalog,
												   databaseSchemaName = DatabaseSchema,
												   databaseUsername = KullaniciAdi,
												   databasePassword = Parola,
												   plmServerAdress = WindchillServerName,
												   plmUserName = BasicUsername,
												   plmPassword = BasicPassword,
												   epmFileName = CADViewResponseContentInfo.name,
												   epmDocNumber = EPMDocID,
												   epmFileContent = CADViewResponseContentInfo.content,
											   }
										   );

											LogService logService = new LogService(_configuration);

											logService.CreateJsonFileLog(LogJsonData, "CAD Döküman bilgileri gönderildi." + dataResponse.message);

											//await SendPdfToCustomerApiAsync(pdfBytes, pdfFileName, customerApiEndpoint, CADViewResponseContentInfo);


											await conn.ExecuteAsync($@"
                                        DELETE FROM [{catalogValue}].[Des_EPMDocument_LogTable]
                                        WHERE EPMDocID = @Ids", new { Ids = EPMDocID });
							

										}
										catch (Exception ex)
										{

											var CADViewResponseContentInfoCatch = new TeknikResim2ViewModel
											{
												Number = "TR_" + CADResponse.Number,
												Revizyon = CADResponse.Revision,
												DocumentType = "TR",
												//Description = CADResponse.Description,
												Description = partName,
												ModifiedOn = CADResponse.LastModified,
												AuthorizationDate = CADResponse.LastModified,
												//ModifiedBy = CADResponse.ModifiedBy,
												ModifiedBy = "WindchillAD",
												state = 30,
												name = pdfFileName,
												content = await DownloadPdfAsync(pdfUrl),
												projectCode = response.ProjeKodu,




											};
											var LogJsonDataCatch = JsonConvert.SerializeObject(CADViewResponseContentInfo);
											LogService logService = new LogService(_configuration);
											logService.CreateJsonFileLog(LogJsonData, "HATA ! " + ex.Message);
										
										

												var Ent_EPMDocStateModelResponse = await conn.QueryFirstAsync<Ent_EPMDocStateModel>(
						  $"SELECT * FROM [{catalogValue}].[Des_EPMDocument_LogTable] WHERE [EPMDocID] = {EPMDocID}");

												var existingErrorRecord = await conn.QueryFirstOrDefaultAsync<Ent_EPMDocStateModel>(
		$"SELECT * FROM [{catalogValue}].[Des_EPMDocument_LogTable_Error] WHERE [EPMDocID] = @EPMDocID AND [idA3masterReference] = @idA3masterReference",
		new { EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID, idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference }
	);



												if (existingErrorRecord == null)
												{
													// Veri yoksa yeni bir kayıt ekle
													await conn.ExecuteAsync(
														$"INSERT INTO [{catalogValue}].[Des_EPMDocument_LogTable_Error] ([EPMDocID],[StateDegeri], [idA3masterReference], [CadName],[name], [docNumber]) VALUES (@EPMDocID,@StateDegeri, @idA3masterReference, @CadName,@name, @docNumber)",
														new
														{
															EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID,
															StateDegeri = Ent_EPMDocStateModelResponse.StateDegeri,
															idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference,
															CadName = Ent_EPMDocStateModelResponse.CadName,
															name = Ent_EPMDocStateModelResponse.CadName,
															docNumber = Ent_EPMDocStateModelResponse.docNumber
														}
													);
												}
												else
												{
													// Veri varsa güncelle
													await conn.ExecuteAsync(
														$"UPDATE [{catalogValue}].[Des_EPMDocument_LogTable_Error] SET [StateDegeri] = @StateDegeri, [CadName] = @CadName, [name] = @name, [docNumber] = @docNumber WHERE [EPMDocID] = @EPMDocID AND [idA3masterReference] = @idA3masterReference",
														new
														{
															EPMDocID = Ent_EPMDocStateModelResponse.EPMDocID,
															StateDegeri = Ent_EPMDocStateModelResponse.StateDegeri,
															idA3masterReference = Ent_EPMDocStateModelResponse.idA3masterReference,
															CadName = Ent_EPMDocStateModelResponse.CadName,
															name = Ent_EPMDocStateModelResponse.CadName,
															docNumber = Ent_EPMDocStateModelResponse.docNumber
														}
													);
												}
											


											await conn.ExecuteAsync($@"
                                        DELETE FROM [{catalogValue}].[Des_EPMDocument_LogTable]
                                        WHERE EPMDocID = @Ids", new { Ids = EPMDocID });
										}


									}
									else
									{
										LogService logService = new LogService(_configuration);
										var jsonData4 = JsonConvert.SerializeObject(CADResponse);
										//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
										logService.CreateJsonFileLogError(jsonData4, $"Released işlemi gerçekleştirildi WTPart state durumu released değil. WTPart Name : {partName} - WTPart Number {partNumber} - WTPart State {partState}");
									}

									}
									else
									{

										await conn.ExecuteAsync($@"
                                        DELETE FROM [{catalogValue}].[Des_EPMDocument_LogTable]
                                        WHERE EPMDocID = @Ids", new { Ids = EPMDocID });
									}
								}
								else
								{
									LogService logService = new LogService(_configuration);
									var jsonData4 = JsonConvert.SerializeObject(CADResponse);
									//logService.CreateJsonFileLog(jsonData4, "Attachment da Veri bulunamadı.");
									logService.CreateJsonFileLogError(jsonData4, "Released işlemi gerçekleştirildi ama gönderilmedi. WTPart ilişkisi bulunmadı.");
								}

								//await SendPdfToCustomerApiAsync(pdfBytes, pdfFileName, customerApiEndpoint, CADViewResponseContentInfo);
							}

						}
						catch (Exception ex)
						{
							
							LogService logService = new LogService(_configuration);
							var jsonData4 = JsonConvert.SerializeObject(CADResponse);
							logService.CreateJsonFileLogError(jsonData4, "HATA :" + ex.Message);
						}

					}




				}


			}
			catch (Exception ex)
			{
				LogService logService = new LogService(_configuration);
				var jsonData4 = JsonConvert.SerializeObject(CADResponse);
				logService.CreateJsonFileLogError(jsonData4, "HATA :" + ex.Message);
			}

		}


		public async Task WTDocumentAttachmentFunc(string fileContentBase64, string AttachFileName, string CADResponseID)
		{
			try
			{
				WindchillApiService _windchillApiService = new WindchillApiService();
				LogService logService = new LogService(_configuration);


				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}

				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();

				var ServerName = jsonObject["ServerName"].ToString();
				var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();
				var DesVeriTasimaID = jsonObject["DesVeriTasimaID"].ToString();

				if (string.IsNullOrEmpty(catalogValue) || string.IsNullOrEmpty(connectionString) ||
					  string.IsNullOrEmpty(WindchillServerName) || string.IsNullOrEmpty(BasicUsername) ||
					  string.IsNullOrEmpty(BasicPassword) || string.IsNullOrEmpty(DesVeriTasimaID))
				{
					// Hata yönetimi: Loglama, hata fırlatma vb.
					logService.CreateJsonFileLogError(AttachFileName, "ATTACHMENTS DUZENLEME - Ayar dosyasından gerekli bilgiler alınamadı.");
					throw new Exception("ATTACHMENTS DUZENLEME - Ayar dosyasından gerekli bilgiler alınamadı.");
				}

				//GetToken
				WrsToken apiToken = await _windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);
				if (apiToken?.NonceValue is null)
				{
					logService.CreateJsonFileLogError(AttachFileName, "ATTACHMENTS DUZENLEME - Token alınamadı.");
					throw new Exception("ATTACHMENTS DUZENLEME - Token alınamadı.");
				}

				var token = apiToken.NonceValue;

				string jsonContent = "{ \"NoOfFiles\": 1 }";
				var UploadStage1ActionQuery = await _windchillApiService.WTDoc_PostData(WindchillServerName, $"DocMgmt/Documents('OR:wt.doc.WTDocument:{DesVeriTasimaID}')/PTC.DocMgmt.UploadStage1Action", BasicUsername, BasicPassword, token, jsonContent);
				var UploadStage1ActionResponse = JsonConvert.DeserializeObject<CacheDescriptor>(UploadStage1ActionQuery);
				if (UploadStage1ActionResponse?.Value?.FirstOrDefault() is null || UploadStage1ActionResponse.Value.Count == 0)
				{
					logService.CreateJsonFileLogError(AttachFileName, "ATTACHMENTS DUZENLEME - UploadStage1Action başarısız.");
					throw new Exception("ATTACHMENTS DUZENLEME - UploadStage1Action başarısız.");
				}

				var replicaUrlDataQuery = await _windchillApiService.WTDoc_ReplicaUrlPostData2(
					UploadStage1ActionResponse.Value.FirstOrDefault().ReplicaUrl,
					UploadStage1ActionResponse.Value.FirstOrDefault().FileNames.FirstOrDefault(),
					UploadStage1ActionResponse.Value.FirstOrDefault().StreamIds.FirstOrDefault(),
					WindchillServerName,
					BasicUsername,
					BasicPassword,
					token,
					fileContentBase64,
					AttachFileName,
					UploadStage1ActionResponse.Value.FirstOrDefault().MasterUrl
				);
				var replicaUrlDataResponse = JsonConvert.DeserializeObject<ContentInfoResponse>(replicaUrlDataQuery);

				if (replicaUrlDataResponse?.ContentInfos?.FirstOrDefault() is null || replicaUrlDataResponse.ContentInfos.Count == 0)
				{
					// Hata yönetimi: Loglama, hata fırlatma vb.
					logService.CreateJsonFileLogError(AttachFileName, "ATTACHMENTS DUZENLEME - ReplicaUrlData çağrısı başarısız.");
					throw new Exception("ATTACHMENTS DUZENLEME - ReplicaUrlData çağrısı başarısız.");
				}

				var contentInfoList = new List<ContentInfoStage3>
		{
			new ContentInfoStage3
			{
				StreamId = Int32.Parse(replicaUrlDataResponse.ContentInfos.FirstOrDefault().StreamId),
				EncodedInfo = replicaUrlDataResponse.ContentInfos.FirstOrDefault().EncodedInfo,
				FileName = AttachFileName,
				PrimaryContent = false,
				MimeType = "text/plain",
				FileSize = replicaUrlDataResponse.ContentInfos.FirstOrDefault().FileSize
			}
		};

				var rootObject = new ContentInfoStage3RootObject
				{
					ContentInfo = contentInfoList
				};

				string jsonContentStage3 = JsonConvert.SerializeObject(rootObject, Formatting.Indented);



				// SQL İşlemleri

				using (var conn = new SqlConnection(connectionString))
				{
					await conn.OpenAsync();

					// SQL_ApplicationDataQueryResponse'u burada tanımlayın
					dynamic SQL_ApplicationDataQueryResponse = null;

					using (var transaction = conn.BeginTransaction())
					{
						try
						{
							var SQL_ApplicationDataQuery = $"SELECT TOP (10) * FROM {catalogValue}.ApplicationData WHERE role = 'SECONDARY' AND [fileName] = @AttachFileName ORDER BY idA2A2 DESC";
							SQL_ApplicationDataQueryResponse = await conn.QueryFirstOrDefaultAsync<dynamic>(SQL_ApplicationDataQuery, new { AttachFileName }, transaction);

							if (SQL_ApplicationDataQueryResponse != null)
							{
								var UploadStage3ActionQuery = await _windchillApiService.WTDoc_PostData(WindchillServerName, $"DocMgmt/Documents('OR:wt.doc.WTDocument:{DesVeriTasimaID}')/PTC.DocMgmt.UploadStage3Action", BasicUsername, BasicPassword, token, jsonContentStage3);
								var UploadStage3ActionResponse = JsonConvert.DeserializeObject<ApplicationData>(UploadStage3ActionQuery);

								if (UploadStage3ActionResponse?.Value?.FirstOrDefault() is null || UploadStage3ActionResponse.Value.Count == 0)
								{
									// Hata yönetimi: Loglama, hata fırlatma vb.
									logService.CreateJsonFileLogError(AttachFileName, "ATTACHMENTS DUZENLEME - UploadStage3Action başarısız.");
									throw new Exception("ATTACHMENTS DUZENLEME - UploadStage3Action başarısız.");

								}
								await conn.ExecuteAsync(
									$"UPDATE [{catalogValue}].[HolderToContent] SET [classnamekeyroleAObjectRef] = 'wt.epm.EPMDocument', [idA3A5] = @IdA3A5 WHERE [idA3B5] = @IdB3B5",
									new
									{
										IdA3A5 = CADResponseID.Split(':')[2],
										IdB3B5 = UploadStage3ActionResponse.Value.FirstOrDefault().ID.Split(':')[2]
									}, transaction);

								await conn.ExecuteAsync(
									$"UPDATE [{catalogValue}].[HolderToContent] SET [classnamekeyroleAObjectRef] = 'wt.doc.WTDocument', [idA3A5] = @IdA3A5 WHERE [idA3B5] = @IdB3B5",
									new
									{
										IdA3A5 = DesVeriTasimaID,
										IdB3B5 = SQL_ApplicationDataQueryResponse.idA2A2
									}, transaction);
							}

							transaction.Commit();
						}
						catch (Exception ex)
						{
							transaction.Rollback();
							logService.CreateJsonFileLogError(AttachFileName, "ATTACHMENTS DUZENLEME - UploadStage3Action başarısız." + ex.Message);
							throw;
						}
					}

					// Transaction dışına taşınmış API çağrısı
					if (SQL_ApplicationDataQueryResponse != null)
					{
						await _windchillApiService.WTDoc_Delete(WindchillServerName,
							$"DocMgmt/Documents('OR:wt.doc.WTDocument:{DesVeriTasimaID}')/Attachments('OR:wt.content.ApplicationData:{SQL_ApplicationDataQueryResponse.idA2A2}')",
							BasicUsername, BasicPassword, token);

						logService.CreateJsonFileLogError(AttachFileName, "Attachment düzenlemesi windchille aktarıldı.");
					}
				}




			}
			catch (Exception ex)
			{
				LogService logService = new LogService(_configuration);
				logService.CreateJsonFileLogError(AttachFileName, "Attachment düzenlemesi windchille aktarılamadı. HATA : " + ex.Message);
			}

		}





		public async Task<string> DownloadPdfAsync(string pdfUrl)
		{
			try
			{

				string directoryPath = "Configuration";
				string fileName2 = "appsettings.json";
				string fileName3 = "scanpdf-425313-50117e72a809.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName2);
				string filePathPDFScan = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName3);
				byte[] pdfBytes;
				byte[] pdfBytesConvertA4;


				string jsonFilePath = filePathPDFScan;
				GoogleCredential credential = GoogleCredential.FromFile(jsonFilePath);

				// Vision API istemcisini oluşturun
				var clientBuilder = new ImageAnnotatorClientBuilder
				{
					CredentialsPath = jsonFilePath
				};
				var client = clientBuilder.Build();
				//string directoryPath2 = "Configuration";
				//string fileName2 = "ApiSendDataSettings.json";

				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}



				// (Önceki kodlar burada)
				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

				JObject jsonObject = JObject.Parse(jsonData);

				var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();


				var _httpClient1 = new HttpClient();
				_httpClient1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{BasicUsername}:{BasicPassword}")));

				_httpClient1.DefaultRequestHeaders.Add("CSRF-NONCE", CSRF_NONCE);
				using (var response = await _httpClient1.GetAsync(pdfUrl))
				{


					if (response.IsSuccessStatusCode)
					{


						var dosyaAdi = Path.GetFileName(new Uri(pdfUrl).LocalPath);

						// PDF dosyasını belirtilen dizine kaydet
						string saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "PDF");
						string savePath = Path.Combine(saveDirectory, dosyaAdi);

						// Klasör yoksa oluştur
						if (!Directory.Exists(saveDirectory))
						{
							Directory.CreateDirectory(saveDirectory);
						}

						// PDF dosyasını kaydet
						await using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
						{
							await response.Content.CopyToAsync(fileStream);
						}





						byte[] bytes = await response.Content.ReadAsByteArrayAsync();
						//pdfBytesConvertA4 = await ConvertPdfToA4(bytes);

						var stream = new MemoryStream(bytes);
						//PdfiumViewer.PdfDocument pdfDocument = PdfiumViewer.PdfDocument.Load(stream);

						// PDF'yi yükle

						using (var pdfDocument = PdfiumViewer.PdfDocument.Load(stream))
						{

							// Toplam sayfa sayısını belirle
							int totalPages = pdfDocument.PageCount;

							// Sayfaları işle
							List<Tuple<int, string, Bitmap>> sayfaBilgileri = new List<Tuple<int, string, Bitmap>>();
							for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
							{
								using (var page = pdfDocument.Render(pageIndex, 300, 300, PdfRenderFlags.CorrectFromDpi))
								{
									// Sayfayı resim olarak dönüştür
									Bitmap pageImage = ConvertPdfPageToImage(pdfDocument, pageIndex);





									// Görüntü işleme
									//Bitmap processedImage = PreprocessImage(croppedImage);
									System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(5562, 4790, 1400, 38);
									Bitmap croppedBitmap = new Bitmap(cropRect.Width, cropRect.Height);
									using (Graphics g = Graphics.FromImage(croppedBitmap))
									{
										g.DrawImage(page, new System.Drawing.Rectangle(0, 0, croppedBitmap.Width, croppedBitmap.Height),
														 cropRect,
														 GraphicsUnit.Pixel);
									}
									string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"croppedImage-{pageIndex}-.png");
									croppedBitmap.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);

									//Kırpılmış bölgeyi OCR ile tarat
									string ocrResult = PerformOcr(croppedBitmap);
									string sheetInfo = ExtractSheetInfo(ocrResult);

									// Sayfa bilgisini çıkar
									//string sheetInfo = ExtractSheetInfo(ocrResult);


									// Sayfa bilgisini ve resmi listeye ekle
									sayfaBilgileri.Add(Tuple.Create(pageIndex + 1, sheetInfo, pageImage));


									//// Belleği temizle
									croppedBitmap.Dispose();
								}
							}

							// Sayfaları sheet numarasına göre sırala
							sayfaBilgileri.Sort((a, b) =>
							{
								// Boş dize kontrolü ekleyerek güvenli dönüşüm yapın
								string[] aParts = a.Item2.Split(' ');
								string[] bParts = b.Item2.Split(' ');

								if (aParts.Length > 0 && bParts.Length > 0)
								{
									if (int.TryParse(aParts[0], out int aNumber) && int.TryParse(bParts[0], out int bNumber))
									{
										return aNumber - bNumber;
									}
								}

								// Varsayılan olarak sıralamada değişiklik yapmayın
								return 0;
							});



							// Yeni PDF oluştur
							using (PdfSharp.Pdf.PdfDocument newPdfDocument = new PdfSharp.Pdf.PdfDocument())
							{
								foreach (var sayfaBilgisi in sayfaBilgileri)
								{
									Bitmap pageImage = sayfaBilgisi.Item3;
									PdfSharp.Pdf.PdfPage pdfPage = newPdfDocument.AddPage();
									pdfPage.Width = XUnit.FromPoint(pageImage.Width);
									pdfPage.Height = XUnit.FromPoint(pageImage.Height);

									using (XGraphics gfx = XGraphics.FromPdfPage(pdfPage))
									{
										using (MemoryStream ms = new MemoryStream())
										{
											pageImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
											XImage xImage = XImage.FromStream(ms);
											//gfx.DrawImage(xImage, 0, 0);

											// Görüntüyü tam sayfa boyutuna sığdırmak için `DrawImage` kullanın
											gfx.DrawImage(xImage, 0, 0, pdfPage.Width, pdfPage.Height);
										}
									}

									// Belleği temizle
									pageImage.Dispose();
								}

								// Yeni PDF'yi kaydet
								using (MemoryStream ms = new MemoryStream())
								{
									newPdfDocument.Save(ms, false);
									pdfBytes = ms.ToArray();

									// pdfBytes adlı byte dizisini API'ye gönderin
								}
								//newPdfDocument.Save(outputPdfPath);
								Console.WriteLine("PDF başarıyla sıralandı ve kaydedildi.");
							}

						}

						string base64EncodedPdf = Convert.ToBase64String(pdfBytes);
						var content2 = new ByteArrayContent(pdfBytes);

						// Dosya adını Content-Disposition başlığına ekleyin
						content2.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
						{
							FileName = dosyaAdi // Orijinal dosya adını kullanın
						};
						return base64EncodedPdf;




					}
					else
					{
						// Hata durumunu ele al
						throw new Exception($"PDF indirme başarısız. StatusCode: {response.StatusCode}");
					}
				}
			}
			catch (Exception ex)
			{
				// Hata durumunu ele al 
				throw new Exception($"PDF indirme hatası: {ex.Message}");
			}

		}


		public async Task<string> projectCodeRootObjectInfo(string partCode, RootObject CADResponse)
		{
			try
			{
				WindchillApiService windchillApiService = new WindchillApiService();

				string directoryPath = "Configuration";
				string fileName2 = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName2);

				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}

				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
				JObject jsonObject = JObject.Parse(jsonData);
				var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();
				var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();

				var apiData = await windchillApiService.GetApiData(WindchillServerName, $"ProdMgmt/Parts('OR:wt.part.WTPart:{partCode}')", BasicUsername, BasicPassword, CSRF_NONCE);

				// API yanıtı boş veya geçersiz ise logla ve hata fırlat
				if (string.IsNullOrEmpty(apiData))
				{
					var hataMesajı = "Windchill API'sinden veri alınamadı. Yanıt boş.";
					LogService logService = new LogService(_configuration);
					var jsonData4 = JsonConvert.SerializeObject(CADResponse);
					logService.CreateJsonFileLogError(jsonData4, hataMesajı);
					throw new Exception(hataMesajı);
				}

				return apiData; // Başarılı durumda API verilerini döndür
			}
			catch (Exception ex)
			{
				LogService logService = new LogService(_configuration);
				var jsonData4 = JsonConvert.SerializeObject(CADResponse);
				logService.CreateJsonFileLogError(jsonData4, $"projectCodeInfo fonksiyonunda hata oluştu: {ex.Message}");
				// Hata durumunda boş bir string döndür veya uygun bir hata yönetimi uygulayın
				return string.Empty;
			}

		}




		#region PDF SIRALI YAPMA AYARLARI VS.
		private static Bitmap ResizeImage(Bitmap image, int width, int height)
		{
			Bitmap resizedImage = new Bitmap(width, height);
			using (Graphics g = Graphics.FromImage(resizedImage))
			{
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(image, 0, 0, width, height);
			}
			return resizedImage;
		}
		public static Bitmap ConvertPdfPageToImage(PdfiumViewer.PdfDocument pdfDocument, int pageIndex)
		{
			int dpi = 900;
			using (var page = pdfDocument.Render(pageIndex, dpi, dpi, true))
			{
				return new Bitmap(page);
			}
		}

		public static Bitmap CropImage(Bitmap source, System.Drawing.Rectangle cropArea)
		{
			Bitmap croppedImage = new Bitmap(cropArea.Width, cropArea.Height);

			using (Graphics g = Graphics.FromImage(croppedImage))
			{
				g.DrawImage(source, new System.Drawing.Rectangle(0, 0, cropArea.Width, cropArea.Height), cropArea, GraphicsUnit.Pixel);
			}

			return croppedImage;
		}



		// Görüntü ön işleme: Gri tonlama, binaryzasyon, kontrast artırma
		private static Bitmap PreprocessImage(Bitmap image)
		{


			// Gri tonlamaya çevirme
			Bitmap grayImage = new Bitmap(image.Width, image.Height);
			using (Graphics g = Graphics.FromImage(grayImage))
			{
				ColorMatrix colorMatrix = new ColorMatrix(new float[][]
				{
				new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
				new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
				new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
				new float[] { 0, 0, 0, 1, 0 },
				new float[] { 0, 0, 0, 0, 1 }
				});

				ImageAttributes attributes = new ImageAttributes();
				attributes.SetColorMatrix(colorMatrix);

				g.DrawImage(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
			}

			// Kontrast artırma ve binaryzasyon
			for (int y = 0; y < grayImage.Height; y++)
			{
				for (int x = 0; x < grayImage.Width; x++)
				{
					Color pixelColor = grayImage.GetPixel(x, y);
					int grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
					Color newColor = (grayValue > 128) ? Color.White : Color.Black;
					grayImage.SetPixel(x, y, newColor);
				}
			}

			return grayImage;
		}

		public static Bitmap AdjustContrast(Bitmap image, double contrast)
		{
			// Kontrastı ayarlamak için bir formül
			// Değer 1'den büyükse kontrast artar, 1'den küçükse azalır. 
			double factor = (259 * (contrast + 255)) / (255 * (259 - contrast));
			Bitmap contrastImage = new Bitmap(image.Width, image.Height);

			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					Color c = image.GetPixel(x, y);
					int r = (int)Math.Min(Math.Max((c.R - 128) * factor + 128, 0), 255);
					int g = (int)Math.Min(Math.Max((c.G - 128) * factor + 128, 0), 255);
					int b = (int)Math.Min(Math.Max((c.B - 128) * factor + 128, 0), 255);
					contrastImage.SetPixel(x, y, Color.FromArgb(r, g, b));
				}
			}

			return contrastImage;
		}
		public static Bitmap GrayscaleImage(Bitmap image)
		{
			Bitmap grayImage = new Bitmap(image.Width, image.Height);

			using (Graphics g = Graphics.FromImage(grayImage))
			{
				// Gri tonlama
				ColorMatrix colorMatrix = new ColorMatrix(
					new float[][]
					{
						new float[] { 0.3f, 0.3f, 0.3f, 0, 0 },
						new float[] { 0.59f, 0.59f, 0.59f, 0, 0 },
						new float[] { 0.11f, 0.11f, 0.11f, 0, 0 },
						new float[] { 0, 0, 0, 1, 0 },
						new float[] { 0, 0, 0, 0, 1 }
					});

				ImageAttributes attributes = new ImageAttributes();
				attributes.SetColorMatrix(colorMatrix);

				g.DrawImage(image, new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
					0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
			}

			return grayImage;
		}

		public static Bitmap ApplyConvolutionFilter(Bitmap source, double[,] kernel)
		{
			Bitmap result = new Bitmap(source.Width, source.Height);

			int kernelWidth = kernel.GetLength(1);
			int kernelHeight = kernel.GetLength(0);
			int kernelHalfWidth = kernelWidth / 2;
			int kernelHalfHeight = kernelHeight / 2;

			for (int y = kernelHalfHeight; y < source.Height - kernelHalfHeight; y++)
			{
				for (int x = kernelHalfWidth; x < source.Width - kernelHalfWidth; x++)
				{
					double blue = 0.0, green = 0.0, red = 0.0;

					for (int filterY = 0; filterY < kernelHeight; filterY++)
					{
						for (int filterX = 0; filterX < kernelWidth; filterX++)
						{
							int calcX = x + filterX - kernelHalfWidth;
							int calcY = y + filterY - kernelHalfHeight;

							if (calcX >= 0 && calcX < source.Width && calcY >= 0 && calcY < source.Height)
							{
								Color sourcePixel = source.GetPixel(calcX, calcY);
								blue += (double)(sourcePixel.B) * kernel[filterY, filterX];
								green += (double)(sourcePixel.G) * kernel[filterY, filterX];
								red += (double)(sourcePixel.R) * kernel[filterY, filterX];
							}
						}
					}

					int resultR = (int)Math.Min(Math.Max((int)red, 0), 255);
					int resultG = (int)Math.Min(Math.Max((int)green, 0), 255);
					int resultB = (int)Math.Min(Math.Max((int)blue, 0), 255);

					result.SetPixel(x, y, Color.FromArgb(resultR, resultG, resultB));
				}
			}

			return result;
		}

		public static Bitmap ThresholdImage(Bitmap image)
		{
			Bitmap thresholdImage = new Bitmap(image.Width, image.Height);

			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					Color c = image.GetPixel(x, y);
					byte gray = (byte)(0.3 * c.R + 0.59 * c.G + 0.11 * c.B);
					byte threshold = gray > 128 ? (byte)255 : (byte)0;
					thresholdImage.SetPixel(x, y, Color.FromArgb(threshold, threshold, threshold));
				}
			}

			return thresholdImage;
		}

		public static string PerformOcr(Bitmap image)
		{
			string resultText = string.Empty;

			// Görüntüyü ölçeklendir
			Bitmap resizedImage = ResizeImage(image, image.Width * 5, image.Height * 5); // 1 kat büyütme

			// Görüntü ön işleme adımı
			Bitmap preprocessedImage = PreprocessImage(resizedImage);

			// Tesseract OCR motorunu İngilizce dil desteğiyle başlat ve sadece sayıları tanı
			using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
			{
				// Tesseract'ı sadece sayıları tanıyacak şekilde ayarla
				//engine.SetVariable("tessedit_char_whitelist", "0123456789");
				engine.SetVariable("tessedit_pageseg_mode", "6");
				using (var img = BitmapToPixConverter(preprocessedImage))
				{
					using (var page = engine.Process(img))
					{
						resultText = page.GetText();
					}
				}
			}

			// OCR sonucunu düzelt
			resultText = CorrectOcrResult(resultText);

			return resultText;
		}

		private static string CorrectOcrResult(string ocrResult)
		{
			// Gerekirse burada ek düzeltmeler yapabilirsiniz
			return ocrResult.Trim();
		}


		public static string ExtractSheetInfo(string ocrResult)
		{
			try
			{


				string sheetInfo = "";

				// OCR sonucunu temizle
				string cleanedOcrResult = ocrResult.Replace("\n", " ").Replace("\r", " ").Replace("\\", " ");
				cleanedOcrResult = Regex.Replace(cleanedOcrResult, @"\s+", " "); // Fazla boşlukları tek bir boşluk ile değiştir
				cleanedOcrResult = Regex.Replace(cleanedOcrResult, @"[^a-zA-Z0-9\s]", ""); // Alfabetik ve sayısal olmayan karakterleri kaldır
				cleanedOcrResult = cleanedOcrResult.Trim(); // Başındaki ve sonundaki boşlukları kaldır

				// Temizlenmiş metni kontrol edelim
				Console.WriteLine("Temizlenmiş OCR Sonucu: " + cleanedOcrResult);

				// "SHEET" ile başlayan ve "OF" veya "0F" ile biten kısmı bul
				string[] parts = cleanedOcrResult.Split(new string[] { "I Sayfa I", "Sayfa", "SHEET ", "SHEET", "sHEET", "1SHEET", "I Sayfa" }, StringSplitOptions.None);

				// İlgilendiğimiz kısım ikinci elemandır (SHEET'ten sonraki)
				if (parts.Length > 1)
				{
					string sheetPart = parts[1].Trim();
					// "OF" veya "0F" ile biten kısmı ayır
					string[] sheetNumbers = sheetPart.Split(new string[] { "Topam", "IToplam", "I Toplam", "I Toplam Sayfa", "Toplam", "Toplam Sayfa", "|Top|am Sayfa", "| Top | am Sayfa", "OF", "0F", "or" }, StringSplitOptions.None);

					if (sheetNumbers.Length > 1)
					{
						string sheetNumber = sheetNumbers[0].Trim();
						string totalSheets = sheetNumbers[1].Trim();

						// İstenen çıktıyı oluştur
						sheetInfo = sheetNumber;
						//sheetInfo = $"SHEET {sheetNumber}";
						//sheetInfo = $"SHEET {sheetNumber} OF {totalSheets}";
					}
				}

				return sheetInfo;
			}
			catch (Exception ex)
			{

				MessageBox.Show("HATA : " + ex.Message);
				return "err";
			}
		}

		private static Pix BitmapToPixConverter(Bitmap image)
		{
			using (var memoryStream = new MemoryStream())
			{
				image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
				memoryStream.Position = 0;
				return Pix.LoadFromMemory(memoryStream.ToArray());
			}
		}



		#endregion

		private async Task<string> projectCodeInfo(string partCode, TeknikResim CADResponse)
		{
			try
			{
				WindchillApiService windchillApiService = new WindchillApiService();

				string directoryPath = "Configuration";
				string fileName2 = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName2);

				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}

				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
				JObject jsonObject = JObject.Parse(jsonData);
				var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();
				var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();

				var apiData = await windchillApiService.GetApiData(WindchillServerName, $"ProdMgmt/Parts('OR:wt.part.WTPart:{partCode}')", BasicUsername, BasicPassword, CSRF_NONCE);

				// API yanıtı boş veya geçersiz ise logla ve hata fırlat
				if (string.IsNullOrEmpty(apiData))
				{
					var hataMesajı = "Windchill API'sinden veri alınamadı. Yanıt boş.";
					LogService logService = new LogService(_configuration);
					var jsonData4 = JsonConvert.SerializeObject(CADResponse);
					logService.CreateJsonFileLogError(jsonData4, hataMesajı);
					throw new Exception(hataMesajı);
				}

				return apiData; // Başarılı durumda API verilerini döndür
			}
			catch (Exception ex)
			{
				LogService logService = new LogService(_configuration);
				var jsonData4 = JsonConvert.SerializeObject(CADResponse);
				logService.CreateJsonFileLogError(jsonData4, $"projectCodeInfo fonksiyonunda hata oluştu: {ex.Message}");
				// Hata durumunda boş bir string döndür veya uygun bir hata yönetimi uygulayın
				return string.Empty;
			}
		}








	}
}

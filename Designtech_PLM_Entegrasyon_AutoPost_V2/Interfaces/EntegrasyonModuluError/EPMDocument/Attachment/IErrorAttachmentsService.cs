using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel.CADDocumentMgmt;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.EPMDocument.Attachment
{
	public interface IErrorAttachmentsService
	{
		Task GetErrorAttachments(string state, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword, string sourceApi, string endPoint, int oldAlternateLinkCount, string sablonDataDurumu);
		Task SendPdfToCustomerFunctionAsync(string pdfUrl, string pdfFileName, string apiFullUrl, string apiURL, string endPoint, long EPMDocID, string catalogValue, SqlConnection conn, RootObject CADResponse, string stateType, string partCode);
		Task SendPdfToCustomerAttachmentFunctionAsync(string pdfUrl, string pdfFileName, string apiFullUrl, string apiURL, string endPoint, long EPMDocID, string catalogValue, SqlConnection conn, TeknikResim CADResponse, string stateType, string partCode);
		Task<string> DownloadPdfAsync(string pdfUrl);
		Task WTDocumentAttachmentFunc(string fileContentBase64, string AttachFileName, string CADResponseID);
		Task<string> projectCodeRootObjectInfo(string partCode, RootObject CADResponse);
	}
}

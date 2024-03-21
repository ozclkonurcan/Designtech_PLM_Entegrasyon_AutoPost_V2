using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel
{
	public record WTPartAlternateLink_LOG
	{
		[Key]
        public string ID { get; set; }
        public string? ObjectType { get; set; }
        public string? Name { get; set; }
        public string? Number { get; set; }
        public DateTime updateStampA2 { get; set; }
        public DateTime modifyStampA2 { get; set; }
        public DateTime ProcessTimestamp { get; set; }
        public string? state { get; set; }

    }


	public class WTPartAlternateLink
	{
		public byte? AdministrativeLockIsNull { get; set; }
		public string? TypeAdministrativeLock { get; set; }
		public string? ClassNameKeyDomainRef { get; set; }
		public long? IdA3DomainRef { get; set; }
		public byte? InheritedDomain { get; set; }
		public string? ReplacementType { get; set; }
		public string? ClassNameKeyRoleAObjectRef { get; set; }
		public long? IdA3A5 { get; set; }
		public string? ClassNameKeyRoleBObjectRef { get; set; }
		public long? IdA3B5 { get; set; }
		public string? SecurityLabels { get; set; }
		public DateTime? CreateStampA2 { get; set; }
		public long? MarkForDeleteA2 { get; set; }
		public DateTime? ModifyStampA2 { get; set; }
		public string? ClassNameA2A2 { get; set; }
		[Key]
		public long IdA2A2 { get; set; }
		public int? UpdateCountA2 { get; set; }
		public DateTime? UpdateStampA2 { get; set; }


		public override bool Equals(object obj)
		{
			if (obj is WTPartAlternateLink other)
			{
				// Tüm özellikleri karşılaştırabilirsiniz
				return
			  this.AdministrativeLockIsNull == other.AdministrativeLockIsNull &&
			  this.TypeAdministrativeLock == other.TypeAdministrativeLock &&
			  this.ClassNameKeyDomainRef == other.ClassNameKeyDomainRef &&
			  this.IdA3DomainRef == other.IdA3DomainRef &&
			  this.InheritedDomain == other.InheritedDomain &&
			  this.ReplacementType == other.ReplacementType &&
			  this.ClassNameKeyRoleAObjectRef == other.ClassNameKeyRoleAObjectRef &&
			  this.IdA3A5 == other.IdA3A5 &&
			  this.ClassNameKeyRoleBObjectRef == other.ClassNameKeyRoleBObjectRef &&
			  this.IdA3B5 == other.IdA3B5 &&
			  this.SecurityLabels == other.SecurityLabels &&
			  this.CreateStampA2 == other.CreateStampA2 &&
			  this.MarkForDeleteA2 == other.MarkForDeleteA2 &&
			  this.ModifyStampA2 == other.ModifyStampA2 &&
			  this.ClassNameA2A2 == other.ClassNameA2A2 &&
			  this.IdA2A2 == other.IdA2A2 &&
			  this.UpdateCountA2 == other.UpdateCountA2 &&
			  this.UpdateStampA2 == other.UpdateStampA2;
			}

			return false;
		}

		public override int GetHashCode()
		{
			// Eşitlik karşılaştırmasını sağlamak için GetHashCode'ı geçersiz kılabilirsiniz
			return base.GetHashCode();
		}


	}



	public class WTPartAlternateLinkComparer : IEqualityComparer<WTPartAlternateLink>
	{
		public bool Equals(WTPartAlternateLink x, WTPartAlternateLink y)
		{
			return x.IdA2A2 == y.IdA2A2;
		}

		public int GetHashCode(WTPartAlternateLink obj)
		{
			return obj.IdA2A2.GetHashCode();
		}
	}


	public class WTPartAlternateLinkComparer1 : IEqualityComparer<WTPartAlternateLink>
	{
		public bool Equals(WTPartAlternateLink x, WTPartAlternateLink y)
		{
			if (ReferenceEquals(x, y))
				return true;

			if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
				return false;

			// Tüm özellikleri karşılaştırabilirsiniz
			return
				x.AdministrativeLockIsNull == y.AdministrativeLockIsNull &&
				x.TypeAdministrativeLock == y.TypeAdministrativeLock &&
				x.ClassNameKeyDomainRef == y.ClassNameKeyDomainRef &&
				x.IdA3DomainRef == y.IdA3DomainRef &&
				x.InheritedDomain == y.InheritedDomain &&
				x.ReplacementType == y.ReplacementType &&
				x.ClassNameKeyRoleAObjectRef == y.ClassNameKeyRoleAObjectRef &&
				x.IdA3A5 == y.IdA3A5 &&
				x.ClassNameKeyRoleBObjectRef == y.ClassNameKeyRoleBObjectRef &&
				x.IdA3B5 == y.IdA3B5 &&
				x.SecurityLabels == y.SecurityLabels &&
				x.CreateStampA2 == y.CreateStampA2 &&
				x.MarkForDeleteA2 == y.MarkForDeleteA2 &&
				x.ModifyStampA2 == y.ModifyStampA2 &&
				x.ClassNameA2A2 == y.ClassNameA2A2 &&
				x.IdA2A2 == y.IdA2A2 &&
				x.UpdateCountA2 == y.UpdateCountA2 &&
				x.UpdateStampA2 == y.UpdateStampA2;
		}

		public int GetHashCode(WTPartAlternateLink obj)
		{
			// Tüm özellikleri kullanarak GetHashCode üretebilirsiniz
			unchecked
			{
				int hashCode = obj.AdministrativeLockIsNull?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (obj.TypeAdministrativeLock?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (obj.ClassNameKeyDomainRef?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (obj.IdA3DomainRef?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (obj.InheritedDomain?.GetHashCode() ?? 0);
				// Diğer özellikleri de ekleyin
				return hashCode;
			}
		}
	}

}

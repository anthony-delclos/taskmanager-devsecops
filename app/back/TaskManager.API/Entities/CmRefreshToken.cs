using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COPILmatic_back.API.Entities
{
    public class CmRefreshToken
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("token_hash", TypeName = "char(64)")]
        [StringLength(64)]
        public string TokenHash { get; set; } = null!;

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("revoked")]
        public bool Revoked { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("CmRefreshTokens")]
        public CmUser User { get; set; } = null!;
    }
}

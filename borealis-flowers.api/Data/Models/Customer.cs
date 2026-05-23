using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models
{
    public class Customer
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public string? Phone { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }

        /// <summary>BCrypt hash; заполняется при регистрации через UI.</summary>
        public string? PasswordHash { get; set; }

        public bool IsAdmin { get; set; } = false;

        /// <summary>Одобренная заявка флориста — доступ к кабинету флориста/админа.</summary>
        public bool IsSpecialist { get; set; }

        /// <summary>Связь с записью флориста после одобрения заявки.</summary>
        public Guid? SpecialistId { get; set; }

        public bool IsMaster { get; set; } = false;
        public bool IsAnonymous { get; set; } = false;
        public DateTime FirstVisit { get; set; } = DateTime.UtcNow;
        public DateTime LastVisit { get; set; } = DateTime.UtcNow;
        public string? VisitorId { get; set; }
        public string? ExternalUserId { get; set; }
        public DateTime? Birthday { get; set; }

        /// <summary>Баланс личного счёта клиента (симуляция).</summary>
        public double WalletBalance { get; set; }

        public List<Timeslot>? Timeslots { get; set; }

        public List<PaymentCard>? PaymentCards { get; set; }
    }
}

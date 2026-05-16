using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace borealis_flowers.api.Data.Models;

    public class DateSchedule
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }
        public Guid SpecialistId { get; set; }

        public DateTime Date { get; set; }
        public bool IsWorkingDay { get; set; }
        public bool IsAvailable { get; set; }
        public List<Timeslot> Timeslots { get; set; }
    }


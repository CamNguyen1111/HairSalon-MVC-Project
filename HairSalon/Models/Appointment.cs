using System;
using System.Collections.Generic;

namespace HairSalon.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public int? CustomerId { get; set; }

    public int? StaffId { get; set; }

    public int? ServiceId { get; set; }

    public DateOnly AppointmentDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public string? Status { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Customer { get; set; }

    public virtual Service? Service { get; set; }

    public virtual User? Staff { get; set; }
}

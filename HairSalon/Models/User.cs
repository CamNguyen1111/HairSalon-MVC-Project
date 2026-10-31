using System;
using System.Collections.Generic;

namespace HairSalon.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? RoleId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Appointment> AppointmentCustomers { get; set; } = new List<Appointment>();

    public virtual ICollection<Appointment> AppointmentStaffs { get; set; } = new List<Appointment>();

    public virtual Role? Role { get; set; }
}

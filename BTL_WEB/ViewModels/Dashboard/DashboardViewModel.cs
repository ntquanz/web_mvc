namespace BTL_WEB.ViewModels.Dashboard;

public class DashboardViewModel
{
    public int TotalPets { get; set; }

    public int AvailablePets { get; set; }

    public int AdoptedPets { get; set; }

    public int PendingAppointments { get; set; }

    public int PendingAdoptionRequests { get; set; }

    public decimal PaidPaymentsTotal { get; set; }
}

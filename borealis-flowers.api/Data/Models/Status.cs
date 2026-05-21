namespace borealis_flowers.api.Data.Models;
public enum Status
{
    Booked,
    Completed,
    Wasted,             //Customer didn't call, Master didn't cancel
    CancelledByCustomer,
    CancelledByMaster
}

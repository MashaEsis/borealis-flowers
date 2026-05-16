using System.Reflection;
using System.Security.Claims;
using borealis_flowers.api.Data;
using borealis_flowers.api.Features.Customers;
using borealis_flowers.api.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Features.Statistics;

public static class StatisticsHandler
{
    public static Func<DataContext, Task<List<CustomerDto>>> GetCustomers()
    {
        throw new NotImplementedException();
    }
}

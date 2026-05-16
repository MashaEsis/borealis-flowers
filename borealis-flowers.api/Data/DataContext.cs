using borealis_flowers.api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace borealis_flowers.api.Data
{
    public class DataContext(IConfiguration configuration) : DbContext
    {
        protected readonly IConfiguration Configuration = configuration;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Specialization
            modelBuilder.Entity<Specialization>().HasData(
                    new Specialization
                    {
                        Id = Guid.Parse("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"),
                        Name = "Hair",
                        Description = "Hair Specialization",
                        IsActive = true
                    },
                    new Specialization
                    {
                        Id = Guid.Parse("20df743a-bdfa-48c8-8eee-40ce4a3f3bde"),
                        Name = "Nail",
                        Description = "Nail Specialization",
                        IsActive = true
                    },
                    new Specialization
                    {
                        Id = Guid.Parse("7fe3e393-71d2-4385-b775-8617126c6f0f"),
                        Name = "Skincare",
                        Description = "Skincare Specialization",
                        IsActive = true
                    },
                    new Specialization
                    {
                        Id = Guid.Parse("d78d53c1-f24a-4d27-86a4-54adaebb3ae5"),
                        Name = "Makeup",
                        Description = "Makeup Specialization",
                        IsActive = true
                    }
                );
            #endregion

            #region Specialists
            modelBuilder.Entity<Specialist>().HasData(
                    new Specialist { Id = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"), SpecializationId = Guid.Parse("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"), FullName = "Cecile Hahn", ImgUrl = "https://loremflickr.com/958/958", IsActive = true },
                    new Specialist { Id = Guid.Parse("278666b8-3503-47b0-b5f6-7139563dace6"), SpecializationId = Guid.Parse("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"), FullName = "Francisco Gutkowski", ImgUrl = "http://lorempixel.com/640/480/abstract", IsActive = true },
                    new Specialist { Id = Guid.Parse("b23a6e06-ce61-4445-be74-0cfc5f0a0729"), SpecializationId = Guid.Parse("20df743a-bdfa-48c8-8eee-40ce4a3f3bde"), FullName = "Waino Rath", ImgUrl = "http://lorempixel.com/640/480/nature", IsActive = true },
                    new Specialist { Id = Guid.Parse("5bd8fd04-9613-4c69-85c9-7347572f6289"), SpecializationId = Guid.Parse("7fe3e393-71d2-4385-b775-8617126c6f0f"), FullName = "Emmet Walsh", ImgUrl = "http://lorempixel.com/640/480/sports", IsActive = true },
                    new Specialist { Id = Guid.Parse("88639ec4-d834-4788-bce4-05cfce258cce"), SpecializationId = Guid.Parse("d78d53c1-f24a-4d27-86a4-54adaebb3ae5"), FullName = "Odessa Russel", ImgUrl = "http://lorempixel.com/640/480/animals", IsActive = true }
                );
            #endregion

            #region Services
            modelBuilder.Entity<Service>().HasData(
                    new Service { Id = Guid.Parse("5b385e48-5a16-4575-8290-ee173711840c"), SpecializationId = Guid.Parse("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"), Name = "Coloring", Price = 60d, EstimatedTime = 90 },
                    new Service { Id = Guid.Parse("1219e0a7-4cbb-4395-a7b8-d4ce3979a9ac"), SpecializationId = Guid.Parse("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"), Name = "Haircut", Price = 15d, EstimatedTime = 35 },
                    new Service { Id = Guid.Parse("2944556b-ca71-449c-8b2a-0b493b2d3a78"), SpecializationId = Guid.Parse("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"), Name = "Mens Hair Cut", Price = 19d, EstimatedTime = 35 },
                    new Service { Id = Guid.Parse("ecf67bc9-70f3-4d38-abd6-2e1bbc457756"), SpecializationId = Guid.Parse("ff034503-2dad-402a-a7c0-7aa7f2b2d99b"), Name = "Ladies Cut", Price = 60d, EstimatedTime = 60 },
                    new Service { Id = Guid.Parse("73e26494-9a6c-43cd-a5f0-486afced5d61"), SpecializationId = Guid.Parse("7fe3e393-71d2-4385-b775-8617126c6f0f"), Name = "Makeup", Price = 20d, EstimatedTime = 45 },
                    new Service { Id = Guid.Parse("2dfda2c0-3dc6-4937-b52a-fca938ac6a63"), SpecializationId = Guid.Parse("7fe3e393-71d2-4385-b775-8617126c6f0f"), Name = "Eyebrows", Price = 20d, EstimatedTime = 30 }
                );
            #endregion

            #region DateSchedule
            modelBuilder.Entity<DateSchedule>().HasData(
                new DateSchedule { Id = Guid.Parse("20f92d7b-adec-49c3-88b0-374f45f3e728"), SpecialistId = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"), Date = new DateTime(2023, 7, 13), IsWorkingDay = true, IsAvailable = false },
                new DateSchedule { Id = Guid.Parse("249b8b38-2697-45ba-b59d-839f07af4f51"), SpecialistId = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"), Date = new DateTime(2023, 7, 14), IsWorkingDay = true, IsAvailable = false },
                new DateSchedule { Id = Guid.Parse("e039320c-8ed6-4838-bf13-6b05b9bdcb09"), SpecialistId = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"), Date = new DateTime(2023, 7, 17), IsWorkingDay = true, IsAvailable = false },
                new DateSchedule { Id = Guid.Parse("f91ab0ad-fc6d-4079-a542-92f2be259262"), SpecialistId = Guid.Parse("dfe327cd-3efc-42f5-8dfc-f3bce55a49b7"), Date = new DateTime(2023, 7, 18), IsWorkingDay = true, IsAvailable = false }
                );
            #endregion
        }

        public DbSet<Specialist> Specialists { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServicePrice> ServicePrice { get; set; }
        public DbSet<Specialization> Specialization { get; set; }
        public DbSet<DateSchedule> DateSchedules { get; set; }
        public DbSet<Timeslot> Timeslots { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<HistoryTimeslot> HistoryTimeslots { get; set; }
    }
}

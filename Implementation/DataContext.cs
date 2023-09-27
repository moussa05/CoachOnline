using CoachOnline.Model;
using CoachOnline.Model.Student;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Implementation
{
    public class DataContext : DbContext
    {


        public DataContext()
        {
            this.SaveChangesFailed += DataContext_SaveChangesFailed;
        }
        public DbSet<UserLogins> userLogins { get; set; }
        public DbSet<User> users { get; set; }
        public DbSet<Course> courses { get; set; }
        public DbSet<TwoFATokens> twoFATokens { get; set; }
        public DbSet<Category> courseCategories { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Terms> Terms { get; set; }
        public DbSet<BillingPlan> BillingPlans { get; set; }
        public DbSet<UserBillingPlan> UserBillingPlans { get; set; }
        public DbSet<UserEpisodeAttachemntPermission> UserEpisodeAttachemntPermissions { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<EpisodeAttachment> EpisodeAttachments { get; set; }
        public DbSet<MonthlyBalance> MonthlyBalances { get; set; }
        public DbSet<CoachBalanceMonth> CoachMonthlyBalance { get; set; }
        public DbSet<CoachBalanceDay> CoachDailyBalance { get; set; }
        public DbSet<StudentCourse> StudentOpenedCourses { get; set; }
        public DbSet<StudentEpisode> StudentOpenedEpisodes { get; set; }
        public DbSet<UserWatchedEpisode> UserWatchedEpisodes { get; set; }
        public DbSet<FlaggedCourse> FlaggedCourses { get; set; }
        public DbSet<SuggestedCourse> SuggestedCourses { get; set; }
        public DbSet<PendingCategory> PendingCategories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttachment> EventAttachments { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }
        public DbSet<EventPartner> EventPartners { get; set; }
        public DbSet<Affiliate> Affiliates { get; set; }
        public DbSet<AffiliateLink> AffiliateLinks { get; set; }
        public DbSet<AffiliatePayment> AffiliatePayments { get; set; }
        public DbSet<B2BAccount> B2BAccounts { get; set; }
        public DbSet<B2BSalesPerson> B2BSalesPersons { get; set; }
        public DbSet<B2BPricing> B2BPricings { get; set; }
        public DbSet<B2BAccountService> B2BAccountServices { get; set; }
        public DbSet<B2BAcessToken> B2BAccountTokens { get; set; }
        public DbSet<Profession> Professions { get; set; }
        public DbSet<LibraryAccount> LibraryAccounts { get; set; }
        public DbSet<LibraryReferent> LibraryReferents { get; set; }
        public DbSet<LibraryAcessToken> LibraryAccessTokens { get; set; }
        public DbSet<LibrarySubscription> LibrarySubscriptions { get; set; }
        public DbSet<FAQCategory> FAQCategories { get; set; }
        public DbSet<FAQTopic> FAQTopics { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<PromoCoupon> PromoCoupons { get; set; }
        public DbSet<CourseEval> CourseEvals { get; set; }
        public DbSet<CourseComment> CourseComments { get; set; }
        public DbSet<Questionnaire> Questionnaires { get; set; }
        public DbSet<QuestionnaireOption> QuestionnaireOptions { get; set; }
        public DbSet<QuestionnaireAnswer> QuestionnaireAnswers { get; set; }
        public DbSet<RequestedPayment> RequestedPayments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<WebLink> WebLinks { get; set; }
        public DbSet<UserDocument> UserDocuments { get; set; }

        public void RestartDb()
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        public void CreateDb()
        {
            Database.EnsureCreated();
        }

        public void MigrateDb()
        {
            //RestartDb();
            Database.Migrate();
        }

        private void DataContext_SaveChangesFailed(object sender, SaveChangesFailedEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string hostname = "127.0.0.1";
            string password = "0e47e0f93207df69c81a1f439652cd95";
#if DEBUG
            if (Environment.MachineName == "LENOVxxO-X1YOGA")
            {
               // password = "postgres";
            }
            else
            {
                password = "Ziomal11!";
                hostname = "devops.itsharkz.com";
            }
#endif
            //Console.WriteLine("Using postgres");
            optionsBuilder.UseNpgsql($"Host={hostname};Database=coachonline;Username=postgres;Password={password}",
                options => options.SetPostgresVersion(new Version("9.6")));




            base.OnConfiguring(optionsBuilder);
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<UserBillingPlan>(e =>
            {
                e.HasOne<User>(u => u.User).WithMany(u => u.UserBillingPlans);
                e.HasOne<BillingPlan>(b => b.BillingPlanType).WithMany(b => b.UserBillingPlans);
            });

            modelBuilder.Entity<UserEpisodeAttachemntPermission>(e =>
            {
                e.HasKey(k => new { k.UserId, k.MediaId });
            });

            modelBuilder.Entity<Course>().HasMany(c => c.RejectionsHistory).WithOne(r => r.Course).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<MonthlyBalance>().HasIndex(i => new { i.Month, i.Year });

            modelBuilder.Entity<FlaggedCourse>().HasIndex(i => i.CourseId);

            modelBuilder.Entity<AffiliatePayment>().HasIndex(i => new { i.AffiliateId, i.HostId, i.PaymentForMonth });

            modelBuilder.Entity<B2BAccountService>().HasKey(k => new { k.B2BAccountId, k.ServiceId });

            modelBuilder.Entity<CourseEval>().HasIndex(i => new { i.CourseId, i.UserId });

            base.OnModelCreating(modelBuilder);
        }


    }
}




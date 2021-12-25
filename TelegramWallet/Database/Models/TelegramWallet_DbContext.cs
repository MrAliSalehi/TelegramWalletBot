using Microsoft.EntityFrameworkCore;
using TelegramWallet.Classes;

namespace TelegramWallet.Database.Models
{
    public partial class TelegramWallet_DbContext : DbContext
    {
        public TelegramWallet_DbContext()
        {
        }

        public TelegramWallet_DbContext(DbContextOptions<TelegramWallet_DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<ForceJoinChannel> ForceJoinChannels { get; set; } = null!;
        public virtual DbSet<Question> Questions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Dependencies.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CurrentQuestionLanguage)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UserId)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ForceJoinChannel>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ChId)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ChName)
                    .HasMaxLength(350)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Answer)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreatorId)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Language)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Question1)
                    .HasMaxLength(500)
                    .HasColumnName("Question");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DepositAccount)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.DepositAmount)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.DepositStep).HasDefaultValueSql("((0))");

                entity.Property(e => e.Language)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.ManualAccount)
                    .HasMaxLength(350)
                    .IsUnicode(false);

                entity.Property(e => e.Token).IsUnicode(false);

                entity.Property(e => e.UserId)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UserPass).HasMaxLength(350);

                entity.Property(e => e.WitchDrawPaymentMethod)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.WithDrawAccount)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.WithDrawAmount)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.WithDrawStep).HasDefaultValueSql("((0))");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

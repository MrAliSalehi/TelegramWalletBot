using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Language)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.WithDrawAccount)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UserPass).HasMaxLength(350);

                entity.Property(e => e.WitchDrawPaymentMethod)
                    .HasMaxLength(150)
                    .IsUnicode(false);

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

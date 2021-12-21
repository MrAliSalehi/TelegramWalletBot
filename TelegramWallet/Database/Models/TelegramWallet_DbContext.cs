using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQL2019;Database=TelegramWallet_Db;Integrated Security=True;Connect Timeout=30;User ID=bot;Password=jokerr123");
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

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DepositAmount)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.DepositStep).HasDefaultValueSql("((0))");

                entity.Property(e => e.Language)
                    .HasMaxLength(150)
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

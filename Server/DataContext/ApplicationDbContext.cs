namespace asfalis.Server.DataContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User>? Users { get; set; }
        public DbSet<Image>? Images { get; set; }
        public DbSet<QRCode>? QRCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User table entity 
            modelBuilder.Entity<User>(entity => entity.ToTable("users"));
            modelBuilder.Entity<User>().HasKey(x => x.UserId);
            modelBuilder.Entity<User>().Property(x => x.UserId).HasColumnName("user_id").IsRequired();
            modelBuilder.Entity<User>().HasIndex(x => new { x.Username, x.Email }).IsUnique();
            modelBuilder.Entity<User>().Property(x => x.Username).HasColumnName("username").HasMaxLength(25).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.Email).HasColumnName("email").HasMaxLength(50).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.EmailConfirmed).HasColumnName("email_confirmed").IsRequired().HasDefaultValue(false);
            modelBuilder.Entity<User>().Property(x => x.Gender).HasColumnName("gender").HasColumnType("char").IsRequired();
            modelBuilder.Entity<User>().Property(x => x.Password).HasColumnName("password").IsRequired();
            modelBuilder.Entity<User>().Ignore(x => x.ConfirmPassword);
            modelBuilder.Entity<User>().Property(x => x.LockoutEnd).HasColumnName("lockout_end").IsRequired(false);
            modelBuilder.Entity<User>().Property(x => x.AccessFailedTime).HasColumnName("access_failed_time").IsRequired();

            // Image table entity 
            modelBuilder.Entity<Image>(entity => entity.ToTable("images"));
            modelBuilder.Entity<Image>().HasKey(x => x.ImageId);
            modelBuilder.Entity<Image>().Property(x => x.ImageId).HasColumnName("image_id").IsRequired();
            modelBuilder.Entity<Image>().Property(x => x.Name).HasColumnName("name").HasMaxLength(500).IsRequired();

            // QR Code table entity 
            modelBuilder.Entity<QRCode>(entity => entity.ToTable("qr_codes"));
            modelBuilder.Entity<QRCode>().HasKey(x => x.CodeId);
            modelBuilder.Entity<QRCode>().Property(x => x.CodeId).HasColumnName("code_id").IsRequired();
            modelBuilder.Entity<QRCode>().Property(x => x.OTPCode).HasColumnName("otp_code").HasMaxLength(250).IsRequired();
            modelBuilder.Entity<QRCode>().Property(x => x.ExpiryTime).HasColumnName("expiry_time").IsRequired();
            modelBuilder.Entity<QRCode>().Property(x => x.CodeExpired).HasColumnName("code_expired").IsRequired().HasDefaultValue(false);
            modelBuilder.Entity<QRCode>().Property(x => x.UserId).HasColumnName("user_id").IsRequired();


            // Setup many to many relation between user and images
            modelBuilder.Entity<UserImage>(entity => entity.ToTable("user_image"));
            modelBuilder.Entity<User>()
                .HasMany(u => u.Images)
                .WithMany(i => i.Users)
                .UsingEntity<UserImage>(
                    j => j
                        .HasOne(ui => ui.Image)
                        .WithMany(i => i.UserImages)
                        .HasForeignKey(ui => ui.ImageId),
                    j => j
                        .HasOne(ui => ui.User)
                        .WithMany(u => u.UserImages)
                        .HasForeignKey(ui => ui.UserId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey(ui => new { ui.UserId, ui.ImageId });
                    }
                );

            // Setup the DeleteBehavior for QRCode to be Cascade
            modelBuilder.Entity<QRCode>()
                .HasOne(q => q.User)
                .WithOne(u => u.QRCode)
                .HasForeignKey<QRCode>(q => q.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
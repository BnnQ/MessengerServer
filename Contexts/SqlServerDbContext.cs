using MessengerServer.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Environment = MessengerServer.Configuration.Environment;

namespace MessengerServer.Contexts;

public class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options)
    : IdentityDbContext<User>(options)
{
    public DbSet<Message> Messages { get; set; } = default!;
    public DbSet<Chat> Chats { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.UserName)
                .HasMaxLength(128)
                .IsUnicode()
                .IsRequired();

            entity.Property(user => user.AvatarImagePath)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired(false)
                .HasDefaultValue($"{Environment.HostAddress}/user.png");

            entity.Property(user => user.LastFcmToken)
                .HasMaxLength(512)
                .IsUnicode(false)
                .IsRequired(false);

            entity.HasIndex(user => user.UserName)
                .IsUnique();

            entity.HasMany(user => user.Chats)
                .WithMany(chat => chat.Users);

            entity.ToTable(nameof(Users),
                table =>
                {
                    table.HasCheckConstraint($"CK_{nameof(Users)}_{nameof(User.UserName)}",
                        $"[{nameof(User.UserName)}] IS NULL OR DATALENGTH([{nameof(User.UserName)}]) > 0");
                });
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Text)
                .IsRequired()
                .HasMaxLength(1024)
                .IsUnicode();
            entity.Property(message => message.SentAt)
                .HasDefaultValue(DateTime.UtcNow);
            entity.HasOne(message => message.Sender)
                .WithMany(sender => sender.Messages)
                .HasForeignKey(e => e.SenderId)
                .IsRequired();
            entity.HasOne(message => message.Chat)
                .WithMany(chat => chat.Messages)
                .HasForeignKey(message => message.ChatId)
                .IsRequired();

            entity.ToTable(nameof(Messages),
                table =>
                {
                    table.HasCheckConstraint($"CK_{nameof(Messages)}_{nameof(Message.Text)}",
                        $"DATALENGTH([{nameof(Message.Text)}]) > 0");
                });
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(chat => chat.Id);
            entity.HasMany(chat => chat.Users)
                .WithMany(user => user.Chats);

            entity.HasMany(chat => chat.Messages)
                .WithOne(message => message.Chat)
                .HasForeignKey(message => message.ChatId)
                .IsRequired();

            entity.ToTable(nameof(Chats));
        });
    }
}
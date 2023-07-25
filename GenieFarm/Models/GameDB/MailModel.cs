using System.ComponentModel.DataAnnotations;

public class MailModel
{
    public Int64 MailId { get; set; }

    public Int64 ReceiverId { get; set; }

    public Int64 SenderId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Title must be at least 1 characters long.")]
    [MaxLength(100, ErrorMessage = "Title must be at most 100 characters long.")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Content must be at least 1 characters long.")]
    [MaxLength(2000, ErrorMessage = "Content must be at most 2000 characters long.")]
    public string Content { get; set; } = string.Empty;

    public DateTime ObtainedAt { get; set; }

    public DateTime ExpiredAt { get; set; }

    public bool IsRead { get; set; }

    public bool IsDeleted { get; set; }

    public Int64 ItemId { get; set; }

    public bool IsReceived { get; set; }

    public Int64 Money { get; set; }
}
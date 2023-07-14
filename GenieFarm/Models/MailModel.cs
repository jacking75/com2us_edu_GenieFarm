using System.ComponentModel.DataAnnotations;

public class MailModel
{
    public Int64 MailId { get; set; }

    public Int64 ReceiverId { get; set; }

    public Int64 SenderId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Title must be at least 1 characters long.")]
    [MaxLength(100, ErrorMessage = "Title must be at most 100 characters long.")]
    public String Title { get; set; } = string.Empty;

    [Required]
    [MinLength(1, ErrorMessage = "Content must be at least 1 characters long.")]
    [MaxLength(2000, ErrorMessage = "Content must be at most 2000 characters long.")]
    public String Content { get; set; } = string.Empty;

    public DateTime ObtainedAt { get; set; }

    public DateTime ExpiredAt { get; set; }

    public Boolean IsRead { get; set; }

    public Boolean IsDeleted { get; set; }

    public Int64 ItemId { get; set; }

    public Boolean IsReceived { get; set; }
}
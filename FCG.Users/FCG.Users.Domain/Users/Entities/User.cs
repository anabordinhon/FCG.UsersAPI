using FCG.Users.Domain.Common.Entities;
using FCG.Users.Domain.Common.Enuns;
using FCG.Users.Domain.Users.ValueObjects;

namespace FCG.Users.Domain.Users.Entities
{
    public class User : BaseEntity
    {
        private User(FullName fullName, EmailAddress emailAddress, NickName nickName, string passwordHash, string passwordSalt, EUserRole role)
        {
            FullName = fullName;
            Email = emailAddress;
            NickName = nickName;
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
            Role = role;
        }
        private User() { }
        //public ICollection<GamePurchase> GamePurchases { get; set; } = [];
        //public ICollection<Promotion> Promotions { get; set; } = [];
        public int GamePurchaseId { get; set; }
        public int PromotionId { get; set; }
        public Guid PublicId { get; private set; } = Guid.NewGuid();
        public FullName FullName { get; private set; } = default!;
        public EmailAddress Email { get; private set; } = default!;
        public NickName NickName { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;
        public string PasswordSalt { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public EUserRole Role { get; private set; }

        public static User Create(FullName fullName, EmailAddress emailAddress, NickName nickName, string passwordHash, string passwordSalt, EUserRole role)
        {

            User user = new User(fullName, emailAddress, nickName, passwordHash, passwordSalt, role);
            return user;

        }

        public void UpdatePassword(string password, string passwordIv)
        {
            PasswordHash = password;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}

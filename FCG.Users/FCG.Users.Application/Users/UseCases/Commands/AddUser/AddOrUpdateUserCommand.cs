using FCG.Users.Domain.Common.Enuns;
using FCG.Users.Domain.Users.ValueObjects;

namespace FCG.Users.Application.Users.UseCases.Commands.AddUser
{
    public class AddOrUpdateUserCommand
    {
        private AddOrUpdateUserCommand(Guid? publicId, FullName fullName, EmailAddress email, NickName nickName, RawPassword password, EUserRole role)
        {
            PublicId = publicId;
            Name = fullName;
            Email = email;
            NickName = nickName;
            Password = password;
            Role = role;
        }
        public Guid? PublicId { get; }
        public FullName Name { get; }
        public EmailAddress Email { get; }
        public NickName NickName { get; }
        public RawPassword Password { get; }
        public EUserRole Role { get; set; }
        public static AddOrUpdateUserCommand Create(Guid? publicId, FullName fullName, EmailAddress email, NickName nickName, RawPassword password, EUserRole role)
        {
            return new AddOrUpdateUserCommand(publicId, fullName, email, nickName, password, role);
        }
    }
}

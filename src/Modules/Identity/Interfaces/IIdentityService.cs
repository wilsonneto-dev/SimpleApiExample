using Identity.Dtos;

namespace Identity.Interfaces;

internal interface IIdentityService
{
    Task SignUp(UserSignUpInput input);
    Task<UserLoginOutput> Login(UserLoginInput input);
}

namespace ApiDiac.Services.Interfaces
{
    using ApiDiac.Domain;
    using System.IdentityModel.Tokens.Jwt;

    public interface IIshareAuthService
    {
        public (bool, JwtSecurityToken) DelegationIsValid(DelegationRequestModel delegationRequestModel);
    }
}
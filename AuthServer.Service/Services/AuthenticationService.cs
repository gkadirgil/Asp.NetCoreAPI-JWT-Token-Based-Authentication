using AuthServer.Core.Configuration;
using AuthServer.Core.DTOs;
using AuthServer.Core.Model;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {

        #region Fields
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenService;
        #endregion

        #region Const
        public AuthenticationService(IOptions<List<Client>> optionsClient, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshToken)
        {
            _clients = optionsClient.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenService = userRefreshToken;
        }
        #endregion

        #region Methods
        public async Task<Response<TokenDTO>> CreateTokenByRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<TokenDTO>.Fail("Refresh token not found", 404, true);
            }

            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);

            if (user == null)
            {
                return Response<TokenDTO>.Fail("User Id not found", 404, true);
            }


            var tokenDTO = _tokenService.CreateToken(user);

            existRefreshToken.Code = tokenDTO.RefreshToken;
            existRefreshToken.Expiration = tokenDTO.RefreshTokenExpiration;

            await _unitOfWork.CommitAsync();

            return Response<TokenDTO>.Success(tokenDTO, 200);

        }

        public async Task<Response<TokenDTO>> CreateTokenAsync(LoginDTO loginDTO)
        {

            if (loginDTO == null) throw new ArgumentNullException(nameof(loginDTO));

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null) return Response<TokenDTO>.Fail("Email or Password is wrong", 400, true);

            if (!await _userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                return Response<TokenDTO>.Fail("Email or Password is wrong", 400, true);
            }

            var token = _tokenService.CreateToken(user);

            var userRefreshToken = await _userRefreshTokenService.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

            if (userRefreshToken == null)
            {
                await _userRefreshTokenService.AddAsync(new UserRefreshToken { UserId = user.Id, Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration });
            }

            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }

            await _unitOfWork.CommitAsync();

            return Response<TokenDTO>.Success(token, 200);


        }

        public Response<ClientTokenDTO> CreateTokenByClient(ClientLoginDTO clientLoginDTO)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientLoginDTO.ClientId && x.Secret == clientLoginDTO.ClientSecret);

            if (client == null)
            {
                return Response<ClientTokenDTO>.Fail("Client or ClientSecret not found", 404, true);
            }

            var token = _tokenService.CreateTokenByClient(client);

            return Response<ClientTokenDTO>.Success(token, 200);
        }

        public async Task<Response<NoDataDTO>> RevokeRefreshToken(string refreshToken)
        {
            // If User LogOut in system, RefreshTokne will be null.

            var existRefreshToken = await _userRefreshTokenService.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if (existRefreshToken == null)
            {
                return Response<NoDataDTO>.Fail("Refresh token not found", 404, true);
            }

            _userRefreshTokenService.Remove(existRefreshToken);

            await _unitOfWork.CommitAsync();

            return Response<NoDataDTO>.Success(200);

        }
        #endregion

    }
}

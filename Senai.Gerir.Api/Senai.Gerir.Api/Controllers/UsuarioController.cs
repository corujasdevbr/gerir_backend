using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Senai.Gerir.Api.Dominios;
using Senai.Gerir.Api.Interfaces;
using Senai.Gerir.Api.Repositorios;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Senai.Gerir.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepositorio _usuarioRepositorio;

        public UsuarioController()
        {
            _usuarioRepositorio = new UsuarioRepositorio();
        }

        [HttpPost]
        public IActionResult Cadastrar(Usuario usuario)
        {
            try
            {
                _usuarioRepositorio.Cadastrar(usuario);

                return Ok(usuario);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    
        [HttpPost("login")]
        public IActionResult Logar(Usuario usuario)
        {
            try
            {
                //Verifica se o usuário existe
                var usuarioexiste = _usuarioRepositorio
                                        .Logar(usuario.Email, usuario.Senha);

                //Caso não exista retorna NotFound
                if (usuarioexiste == null)
                    return NotFound();

                //Caso exista gera o token do usuário
                var token = GerarJsonWebToken(usuarioexiste);

                //retorna sucesso com o Token do Usuário
                return Ok(new { token = token});
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Busca as informações do Usuário 
        /// </summary>
        /// <returns>Retorna o usuario</returns>
        [Authorize]
        [HttpGet]
        public IActionResult MeusDados()
        {
            try
            {
                //Pega as informações das claims referente ao usuário
                var claimsUsuario = HttpContext.User.Claims;

                //Pega o id do usuário na Claim Jti
                var usuarioid = claimsUsuario.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

                //Pega as informações do usuário
                var usuario = _usuarioRepositorio.BuscarPorId(new Guid(usuarioid.Value));

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        public IActionResult Editar(Usuario usuario)
        {
            try
            {
                //Pega as informações das claims referente ao usuário
                var claimsUsuario = HttpContext.User.Claims;

                //Pega o id do usuário na Claim Jti
                var usuarioid = claimsUsuario.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

                //Atribuo o valor do usuarioid ao id do usuario recebido
                usuario.Id = new Guid(usuarioid.Value);

                //Envia para o metodo editar os dados do usuário recebido
                _usuarioRepositorio.Editar(usuario);

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpDelete]
        public IActionResult Remover()
        {
            try
            {
                //Pega as informações das claims referente ao usuário
                var claimsUsuario = HttpContext.User.Claims;

                //Pega o id do usuário na Claim Jti
                var usuarioid = claimsUsuario.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

                _usuarioRepositorio.Remover(new Guid(usuarioid.Value));

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string GerarJsonWebToken(Usuario usuario)
        {
            //Chave de Segurança
            var chaveSeguranca = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("GerirChaveSeguranca"));
            //Define as credenciais
            var credenciais = new SigningCredentials(chaveSeguranca, SecurityAlgorithms.HmacSha256);

            var data = new[]
            {
                new Claim(JwtRegisteredClaimNames.FamilyName, usuario.Nome),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Tipo),
                new Claim(JwtRegisteredClaimNames.Jti, usuario.Id.ToString())
            };

            var token = new JwtSecurityToken(
                "gerir.com.br",
                "gerir.com.br",
                data,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credenciais
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
         }
    }
}

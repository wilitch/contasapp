using Bogus;
using ContasApp.Data.Entities;
using ContasApp.Data.Repositories;
using ContasApp.Messages.Models;
using ContasApp.Messages.Services;
using ContasApp.Presentation.Helpers;
using ContasApp.Presentation.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ContasApp.Presentation.Controllers
{
    public class AccountController : Controller
    {
        //GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        //POST: Account/Login
        [HttpPost]
        public IActionResult Login(AccountLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //buscar o usuário no banco de dados através do email e da senha
                    var usuarioRepository = new UsuarioRepository();
                    var usuario = usuarioRepository.GetByEmailAndSenha(model.Email, MD5Helper.Encrypt(model.Senha));

                    //verificar se o usuário foi encontrado
                    if (usuario != null)
                    {
                        //criar os dados que serão gravados no cookie para autenticação do usuário
                        var auth = new AuthViewModel
                        {
                            Id = usuario.Id,
                            Nome = usuario.Nome,
                            Email = usuario.Email,
                            DataHoraAcesso = DateTime.Now
                        };

                        //serializando o objeto AuthViewModel para JSON
                        var authJson = JsonConvert.SerializeObject(auth);

                        //criando o conteúdo do cookie de autenticação (identificação)
                        var identity = new ClaimsIdentity(new[] {
                            new Claim(ClaimTypes.Name, authJson)
                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                        //gravando o cookie de autenticação
                        var principal = new ClaimsPrincipal(identity);
                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        return RedirectToAction("Dashboard", "Principal");
                    }
                    else
                    {
                        TempData["Mensagem"] = "Acesso negado.";
                    }
                }
                catch (Exception e)
                {
                    TempData["Mensagem"] = e.Message;
                }
            }

            return View();
        }

        //GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        //POST: Account/Register
        [HttpPost]
        public IActionResult Register(AccountRegisterViewModel model)
        {
            //verificando se todos os campos enviados pela model
            //passaram nas regras de validação
            if (ModelState.IsValid)
            {
                try
                {
                    //verificando se o email já está cadastrado no banco de dados
                    var usuarioRepository = new UsuarioRepository();
                    if (usuarioRepository.GetByEmail(model.Email) != null)
                    {
                        //gerando uma mensagem de erro na página
                        ModelState.AddModelError("Email", "O email informado já está cadastrado para outro usuário.");
                    }
                    else
                    {
                        //criando um objeto usuário
                        var usuario = new Usuario()
                        {
                            Id = Guid.NewGuid(),
                            Nome = model.Nome,
                            Email = model.Email,
                            Senha = MD5Helper.Encrypt(model.Senha)
                        };

                        //gravando o usuário no banco de dados                    
                        usuarioRepository.Add(usuario);

                        //gerando uma mensagem
                        TempData["Mensagem"] = "Parabéns, sua conta de usuário foi criada com sucesso.";

                        //limpar os campos do formulário
                        ModelState.Clear();
                    }
                }
                catch (Exception e)
                {
                    TempData["Mensagem"] = e.Message;
                }
            }

            return View();
        }

        //GET: Account/PasswordRecover
        public IActionResult PasswordRecover()
        {
            return View();
        }

        //POST: Account/PasswordRecover
        [HttpPost]
        public IActionResult PasswordRecover(AccountPasswordRecoverViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //obter o usuário no banco de dados através do email
                    var usuarioRepository = new UsuarioRepository();
                    var usuario = usuarioRepository.GetByEmail(model.Email);

                    //verificar se o usuário foi encontrado
                    if (usuario != null)
                    {
                        //gerando uma nova senha para o usuário
                        Faker faker = new Faker();
                        var novaSenha = $"@{faker.Internet.Password(8)}{new Random().Next(999)}";

                        //criando o conteudo da mensagem que será enviada por email
                        var emailMessageModel = new EmailMessageModel
                        {
                            EmailDestinatario = usuario.Email,
                            Assunto = "Recuperação de senha de usuário - Contas App",
                            Corpo = $"Prezado, {usuario.Nome},\nSua nova senha de acesso é: {novaSenha}\nAtt\nEquipe Contas App"
                        };

                        //enviando a mensagem
                        EmailMessageService.Send(emailMessageModel);

                        //atualizando a senha do usuário no banco de dados
                        usuario.Senha = MD5Helper.Encrypt(novaSenha);
                        usuarioRepository.Update(usuario);

                        TempData["Mensagem"] = "Recuperação de senha realizada com sucesso. Verifique sua caixa de email.";
                        ModelState.Clear(); //limpar o formulário
                    }
                    else
                    {
                        TempData["Mensagem"] = "Usuário não encontrado. Verifique o email informado.";
                    }
                }
                catch (Exception e)
                {
                    TempData["Mensagem"] = e.Message;
                }
            }

            return View();
        }

        //GET: Account/Logout
        public IActionResult Logout()
        {
            //apagar o cookie de autenticação do AspNet
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //redirecionar para a página /Account/Login
            return RedirectToAction("Login", "Account");
        }
    }
}




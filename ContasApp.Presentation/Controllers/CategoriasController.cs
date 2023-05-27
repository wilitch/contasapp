using ContasApp.Data.Entities;
using ContasApp.Data.Enums;
using ContasApp.Data.Repositories;
using ContasApp.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;

namespace ContasApp.Presentation.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        //GET: /Categorias/Cadastro
        public IActionResult Cadastro()
        {
            //carregando uma ViewBag com a opções que serão exibidas no campo DropDownList
            ViewBag.Tipos = new SelectList(Enum.GetValues(typeof(TipoCategoria)));
            return View();
        }

        //POST: /Categorias/Cadastro
        [HttpPost]
        public IActionResult Cadastro(CategoriasCadastroViewModel model)
        {
            //verificar se todos os campos passaram nas regras de validação
            if (ModelState.IsValid)
            {
                try
                {
                    //capturando os dados do usuário autenticado através do Cookie do AspNet
                    var auth = JsonConvert.DeserializeObject<AuthViewModel>(User.Identity.Name);

                    //preenchendo os dados da categoria para gravar no banco de dados
                    var categoria = new Categoria
                    {
                        Id = Guid.NewGuid(), //chave primária
                        Nome = model.Nome,
                        Tipo = model.Tipo,
                        UsuarioId = auth?.Id //chave estrangeira
                    };

                    //gravando a categoria no banco de dados
                    var categoriaRepository = new CategoriaRepository();
                    categoriaRepository.Add(categoria);

                    //limpar os campos do formulário
                    ModelState.Clear();

                    TempData["MensagemSucesso"] = $"Categoria '{categoria.Nome}', cadastrada com sucesso.";
                }
                catch (Exception e)
                {
                    TempData["MensagemErro"] = e.Message;
                }
            }

            //carregando uma ViewBag com a opções que serão exibidas no campo DropDownList
            ViewBag.Tipos = new SelectList(Enum.GetValues(typeof(TipoCategoria)));
            return View(); //voltando para a página de cadastro
        }

        //GET: /Categorias/Consulta
        public IActionResult Consulta()
        {
            var model = new List<CategoriasConsultaViewModel>();

            try
            {
                //capturando os dados do usuário autenticado através do Cookie do AspNet
                var auth = JsonConvert.DeserializeObject<AuthViewModel>(User.Identity.Name);

                //consultando todas as categorias do usuário autenticado
                var categoriaRepository = new CategoriaRepository();
                foreach (var item in categoriaRepository.GetByUsuario(auth.Id))
                {
                    model.Add(new CategoriasConsultaViewModel
                    {
                        Id = item.Id,
                        Nome = item.Nome,
                        Tipo = item.Tipo
                    });
                }
            }
            catch (Exception e)
            {
                TempData["MensagemErro"] = e.Message;
            }

            //enviando a lista para a página
            return View(model);
        }

        //GET: /Categorias/Exclusao/{id}
        public IActionResult Exclusao(Guid id)
        {
            try
            {
                //buscar a categoria no banco de dados através do ID
                var categoriaRepository = new CategoriaRepository();
                var categoria = categoriaRepository.GetById(id);

                //excluindo a categoria
                categoriaRepository.Delete(categoria);

                TempData["MensagemSucesso"] = $"Categoria '{categoria.Nome}', excluída com sucesso.";
            }
            catch (Exception e)
            {
                TempData["MensagemErro"] = e.Message;
            }

            //redirecionar de volta para a consulta
            return RedirectToAction("Consulta");
        }

        //GET: /Categorias/Edicao
        public IActionResult Edicao(Guid id)
        {
            var model = new CategoriasEdicaoViewModel();

            try
            {
                var categoriaRepository = new CategoriaRepository();
                var categoria = categoriaRepository.GetById(id);

                model.Id = categoria?.Id;
                model.Nome = categoria?.Nome;
                model.Tipo = categoria?.Tipo;
            }
            catch (Exception e)
            {
                TempData["MensagemErro"] = e.Message;
            }

            ViewBag.Tipos = new SelectList(Enum.GetValues(typeof(TipoCategoria)));
            return View(model);
        }

        //POST: /Categorias/Edicao
        [HttpPost]
        public IActionResult Edicao(CategoriasEdicaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //buscar a categoria no banco de dados, através do ID
                    var categoriaRepository = new CategoriaRepository();
                    var categoria = categoriaRepository.GetById(model.Id.Value);

                    categoria.Nome = model.Nome;
                    categoria.Tipo = model.Tipo;

                    //atualizar a categoria
                    categoriaRepository.Update(categoria);

                    TempData["MensagemSucesso"] = $"Categoria '{categoria.Nome}', atualizada com sucesso.";
                                        
                }
                catch (Exception e)
                {
                    TempData["MensagemErro"] = e.Message;
                }
            }

            ViewBag.Tipos = new SelectList(Enum.GetValues(typeof(TipoCategoria)));
            return View(model);
        }
    }
}




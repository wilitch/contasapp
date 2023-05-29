using ContasApp.Data.Entities;
using ContasApp.Data.Repositories;
using ContasApp.Presentation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace ContasApp.Presentation.Controllers
{
    [Authorize]
    public class ContasController : Controller
    {
        //GET: /Contas/Cadastro
        public IActionResult Cadastro()
        {
            ViewBag.Categorias = ObterCategorias();
            return View();
        }

        //POST: /Contas/Cadastro
        [HttpPost]
        public IActionResult Cadastro(ContasCadastroViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //capturando os dados do usuário autenticado através do Cookie do AspNet
                    var auth = JsonConvert.DeserializeObject<AuthViewModel>(User.Identity.Name);

                    //preenchendo os dados da conta
                    var conta = new Conta
                    {
                        Id = Guid.NewGuid(),
                        Nome = model.Nome,
                        Data = model.Data,
                        Valor = model.Valor,
                        Observacoes = model.Observacoes,
                        CategoriaId = model.CategoriaId,
                        UsuarioId = auth?.Id
                    };

                    //gravando no banco de dados
                    var contaRepository = new ContaRepository();
                    contaRepository.Add(conta);

                    TempData["MensagemSucesso"] = $"Conta '{conta.Nome}', cadastrada com sucesso.";
                    ModelState.Clear();
                }
                catch (Exception e)
                {
                    TempData["MensagemErro"] = e.Message;
                }
            }

            ViewBag.Categorias = ObterCategorias();
            return View();
        }

        //GET: /Contas/Consulta
        public IActionResult Consulta()
        {
            var model = new ContasConsultaViewModel();

            //capturando o primeiro e ultimo dia do mês atual
            var dataAtual = DateTime.Now;
            model.DataInicio = new DateTime(dataAtual.Year, dataAtual.Month, 1);
            model.DataFim = model.DataInicio?.AddMonths(1).AddDays(-1);

            model = ConsultarContas(model);
            return View(model);
        }

        //POST: /Contas/Consulta
        [HttpPost]
        public IActionResult Consulta(ContasConsultaViewModel model)
        {
            if (ModelState.IsValid)
            {
                model = ConsultarContas(model);
            }

            return View(model);
        }

        //método para consultar as contas dentro de um periodo de datas
        private ContasConsultaViewModel ConsultarContas(ContasConsultaViewModel model)
        {
            try
            {
                //capturar o usuário autenticado no sistema
                var auth = JsonConvert.DeserializeObject<AuthViewModel>(User.Identity.Name);

                //consultar as contas no bancos de dados
                var contaRepository = new ContaRepository();
                var contas = contaRepository.GetAll(model.DataInicio, model.DataFim, auth.Id);

                //verificar se alguma conta foi obtida
                if (contas.Count > 0)
                {
                    //armazenar as contas no objeto 'model'
                    model.Resultado = new List<ContasConsultaResultadoViewModel>();
                    foreach (var item in contas)
                    {
                        model.Resultado.Add(new ContasConsultaResultadoViewModel
                        {
                            Id = item.Id,
                            Nome = item.Nome,
                            Data = item.Data,
                            Valor = item.Valor,
                            Categoria = item.Categoria?.Nome,
                            Tipo = item.Categoria?.Tipo.ToString(),
                            Observacoes = item.Observacoes
                        });
                    }
                }
                else
                {
                    TempData["MensagemAlerta"] = "Nenhuma conta foi obtida para o período de datas selecionado.";
                }
            }
            catch (Exception e)
            {
                TempData["MensagemErro"] = e.Message;
            }

            return model;
        }

        //GET: /Contas/Edicao/{id}
        public IActionResult Edicao(Guid id)
        {
            var model = new ContasEdicaoViewModel();

            try
            {
                //pesquisar a conta no banco de dados através do ID
                var contaRepository = new ContaRepository();
                var conta = contaRepository.GetById(id);

                //preenchendo a model com os dados da conta
                model.Id = conta.Id;
                model.Nome = conta.Nome;
                model.Data = conta.Data;
                model.Valor = conta.Valor;
                model.Observacoes = conta.Observacoes;
                model.CategoriaId = conta.CategoriaId;
            }
            catch (Exception e)
            {
                TempData["MensagemErro"] = e.Message;
            }

            ViewBag.Categorias = ObterCategorias();
            return View(model);
        }

        //POST: /Contas/Edicao
        [HttpPost]
        public IActionResult Edicao(ContasEdicaoViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var contaRepository = new ContaRepository();
                    var conta = contaRepository.GetById(model.Id.Value);

                    conta.Nome = model.Nome;
                    conta.Valor = model.Valor;
                    conta.Data = model.Data;
                    conta.Observacoes = model.Observacoes;
                    conta.CategoriaId = model.CategoriaId;

                    contaRepository.Update(conta);
                    TempData["MensagemSucesso"] = "Conta atualizada com sucesso.";
                }
                catch (Exception e)
                {
                    TempData["MensagemErro"] = e.Message;
                }
            }

            ViewBag.Categorias = ObterCategorias();
            return View(model);
        }

        //GET: /Contas/Exclusao/{id}
        public IActionResult Exclusao(Guid id)
        {
            try
            {
                var contaRepository = new ContaRepository();
                var conta = contaRepository.GetById(id);

                contaRepository.Delete(conta);

                TempData["MensagemSucesso"] = $"Conta '{conta.Nome}', excluída com sucesso.";
            }
            catch (Exception e)
            {
                TempData["MensagemErro"] = e.Message;
            }

            //redirecionamento
            return RedirectToAction("Consulta");
        }

        /// <summary>
        /// Método para popular o campo DropDownList de seleção de categorias
        /// </summary>
        private List<SelectListItem> ObterCategorias()
        {
            var lista = new List<SelectListItem>(); //lista de itens de seleção

            try
            {
                //capturando os dados do usuário autenticado através do Cookie do AspNet
                var auth = JsonConvert.DeserializeObject<AuthViewModel>(User.Identity.Name);

                //consultar as categorias do usuário autenticado
                var categoriaRepository = new CategoriaRepository();
                var categorias = categoriaRepository.GetByUsuario(auth?.Id);

                foreach (var item in categorias)
                {
                    lista.Add(new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = $"{item.Nome} ({item.Tipo})"
                    });
                }
            }
            catch (Exception e)
            {
                TempData["MensagemErro"] = e.Message;
            }

            return lista;
        }
    }
}




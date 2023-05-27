﻿using ContasApp.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace ContasApp.Presentation.Models
{
    public class CategoriasEdicaoViewModel
    {
        public Guid? Id { get; set; } //campo oculto

        [MaxLength(100, ErrorMessage = "Por favor, informe no máximo {1} caracteres.")]
        [MinLength(8, ErrorMessage = "Por favor, informe no mínimo {1} caracteres.")]
        [Required(ErrorMessage = "Por favor, informe o nome da categoria.")]
        public string? Nome { get; set; }

        [Required(ErrorMessage = "Por favor, selecione o tipo da categoria.")]
        public TipoCategoria? Tipo { get; set; }
    }
}




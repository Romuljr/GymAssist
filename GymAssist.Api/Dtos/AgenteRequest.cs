using System.ComponentModel.DataAnnotations;

namespace GymAssist.Api.Dtos
{
    public record AgenteRequest(

        [MinLength(6, ErrorMessage = "O nome do usuário deve ter pelo menos {1} caracteres. ")]
        [Required(ErrorMessage = "Por favor, informe seu nome. ")]
        string nomeUsuario,

        [MinLength(8, ErrorMessage = "Envie uma pergunta com pelo menos {1} caracteres. ")]
        [Required(ErrorMessage = "Por favor, escreva sua pergunta. ")]
        string pergunta

        );
}

var painel = new Object();

painel.ListaSenhas = function () {
    $.ajax({
        type: "POST",
        timeout: 50000,
        url: "/api/ListaSenhas",
        async: true,
        success: function (jsonRetornado) {

            if (jsonRetornado.senha != null && jsonRetornado.senha != undefined) {
                $("#senha").text(jsonRetornado.senha.senha);
                $("#guiche").text(jsonRetornado.senha.guiche);

                // Exibe o tipo de senha
                var tipoSenha = jsonRetornado.senha.tipo; 
                $("#tipo-senha").text(tipoSenha === 'P' ? 'Preferencial' : 'Normal');
            }
            if (jsonRetornado.senhas != null && jsonRetornado.senhas != undefined) {

                $("#senhasChamadas").html(""); 

                jsonRetornado.senhas.forEach(function (item) {
                    var span = $("<span>", {
                        class: 'senha-chamada-alinhada'
                    });
                    // Inclui o tipo de senha na exibição
                    var tipoChamado = item.tipo; 
                    span.text("Senha: " + item.senha + " - Guichê: " + item.guiche + " (" + (tipoChamado === 'P' ? 'Preferencial' : 'Normal') + ")");

                    $("#senhasChamadas").append(span); 
                });
            }
        },
        error: function (xhr, status, error) {
            console.error('Erro:', error);
        }
    });
    setTimeout(painel.ListaSenhas, 2000);
}

$(function () {
    painel.ListaSenhas();
});

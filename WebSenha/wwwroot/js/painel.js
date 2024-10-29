const painel = {
    ListaSenhas: function () {
        $.ajax({
            type: "POST",
            timeout: 50000,
            url: "/api/ListaSenhas",
            async: true,
            success: function (jsonRetornado) {
                // Atualiza a senha
                if (jsonRetornado.senha) {
                    $("#senha").text(jsonRetornado.senha.senha);
                    $("#guiche").text(jsonRetornado.senha.guiche);

                    // Exibe o tipo de senha
                    const tipoSenha = jsonRetornado.senha.tipo;
                    $("#tipo-senha").text(tipoSenha === 'P' ? 'Preferencial' : 'Normal');
                }

                // Atualiza a lista de senhas chamadas
                if (jsonRetornado.senhas) {
                    const $senhasChamadas = $("#senhasChamadas");
                    $senhasChamadas.empty(); // Limpa o conteúdo antes de adicionar novas senhas

                    $.each(jsonRetornado.senhas, function (index, item) {
                        const tipoChamado = item.tipo;
                        const span = $("<span>", {
                            class: 'senha-chamada-alinhada',
                            text: `Senha: ${item.senha} - Guichê: ${item.guiche} (${tipoChamado === 'P' ? 'Preferencial' : 'Normal'})`
                        });
                        $senhasChamadas.append(span);
                    });
                }
            },
            error: function (xhr, status, error) {
                console.error('Erro:', error);
            }
        });
        setTimeout(this.ListaSenhas.bind(this), 2000); // Mantém o contexto do 'this'
    }
};

$(function () {
    painel.ListaSenhas();
});

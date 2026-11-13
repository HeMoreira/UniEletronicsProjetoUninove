/* ==========================================================================
   LÓGICA JAVASCRIPT: CONTROLE DE ESTOQUE
   Caminho: wwwroot/js/estoque.js
   ========================================================================== */

document.addEventListener("DOMContentLoaded", function () {
    // 1. Destacar visualmente o nível de estoque com base na quantidade
    const linhas = document.querySelectorAll(".tabela-estoque tbody tr:not(.linha-vazia)");

    linhas.forEach(linha => {
        const tdQuantidade = linha.cells[4];
        if (!tdQuantidade) return;

        const quantidade = parseInt(tdQuantidade.innerText, 10);

        if (quantidade <= 5) {
            tdQuantidade.classList.add("qnt-baixa");
        } else if (quantidade > 5 && quantidade <= 12) {
            tdQuantidade.classList.add("qnt-media");
        } else {
            tdQuantidade.classList.add("qnt-normal");
        }
    });

    // 2. Filtro de Busca Dinâmica (ignora a coluna de ações)
    const inputBusca = document.getElementById("inputBusca");
    if (inputBusca) {
        inputBusca.addEventListener("keyup", function () {
            const busca = inputBusca.value.toLowerCase();
            const linhasTabela = document.querySelectorAll(".tabela-estoque tbody tr:not(.linha-vazia)");

            linhasTabela.forEach(linha => {
                const textoLinha = Array.from(linha.cells)
                    .slice(0, 6)
                    .map(td => td.innerText)
                    .join(" ")
                    .toLowerCase();

                if (textoLinha.includes(busca)) {
                    linha.style.display = "";
                    setTimeout(() => {
                        linha.classList.remove("hidden");
                    }, 10);
                } else {
                    linha.classList.add("hidden");
                    setTimeout(() => {
                        if (linha.classList.contains("hidden")) {
                            linha.style.display = "none";
                        }
                    }, 300);
                }
            });
        });
    }

    // 3. Modal de edição
    const modal = document.getElementById("modalEditar");
    const campoId = document.getElementById("editId");
    const campoNome = document.getElementById("editNome");
    const campoCategoria = document.getElementById("editCategoria");
    const campoQuantidade = document.getElementById("editQuantidade");
    const campoPreco = document.getElementById("editPreco");

    function abrirModal(botao) {
        campoId.value = botao.dataset.id;
        campoNome.value = botao.dataset.nome;
        campoCategoria.value = botao.dataset.categoria;
        campoQuantidade.value = botao.dataset.quantidade;
        campoPreco.value = botao.dataset.preco;

        modal.classList.add("aberto");
        modal.setAttribute("aria-hidden", "false");
        campoNome.focus();
    }

    function fecharModal() {
        modal.classList.remove("aberto");
        modal.setAttribute("aria-hidden", "true");
    }

    document.querySelectorAll(".btn-edit").forEach(botao => {
        botao.addEventListener("click", () => abrirModal(botao));
    });

    document.getElementById("btnCancelarEdicao").addEventListener("click", fecharModal);

    modal.addEventListener("click", function (e) {
        if (e.target === modal) fecharModal();
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && modal.classList.contains("aberto")) fecharModal();
    });

    // 4. Confirmação antes de excluir
    document.querySelectorAll(".form-excluir").forEach(form => {
        form.addEventListener("submit", function (e) {
            const nome = form.dataset.nome || "este produto";
            if (!confirm(`Deseja realmente excluir "${nome}"? Essa ação não pode ser desfeita.`)) {
                e.preventDefault();
            }
        });
    });

    // 5. Some com as mensagens de sucesso/erro depois de alguns segundos
    document.querySelectorAll(".alerta").forEach(alerta => {
        setTimeout(() => {
            alerta.style.transition = "opacity 0.4s";
            alerta.style.opacity = "0";
            setTimeout(() => alerta.remove(), 400);
        }, 5000);
    });
});
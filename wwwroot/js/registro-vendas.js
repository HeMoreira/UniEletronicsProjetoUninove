/* ==========================================================================
   LÓGICA JAVASCRIPT: REGISTRO DE VENDAS
   Caminho: wwwroot/js/registro-vendas.js
   ========================================================================== */

document.addEventListener("DOMContentLoaded", function () {
    carregarVendas();
});

function carregarVendas() {
    const vendas = JSON.parse(localStorage.getItem("vendas")) || [];
    const tabela = document.getElementById("tabelaVendas");

    if (!tabela) return;

    tabela.innerHTML = "";
    let total = 0;

    vendas.forEach((venda, index) => {
        total += Number(venda.quantidade);

        tabela.innerHTML += `
            <tr>
                <td>${venda.produto}</td>
                <td>${venda.quantidade}</td>
                <td>${formatarData(venda.data)}</td>
                <td><button type="button" class="btn-excluir" onclick="excluirVenda(${index})">X</button></td>
            </tr>
        `;
    });

    const elemTotal = document.getElementById("total");
    if (elemTotal) {
        elemTotal.innerText = total;
    }
}

function registrarVenda() {
    const produto = document.getElementById("produto").value;
    const quantidade = document.getElementById("quantidade").value;
    const data = document.getElementById("data").value;

    if (!produto || !quantidade || !data) {
        alert("Preencha todos os campos!");
        return;
    }

    const novaVenda = { produto, quantidade, data };

    const vendas = JSON.parse(localStorage.getItem("vendas")) || [];
    vendas.push(novaVenda);

    localStorage.setItem("vendas", JSON.stringify(vendas));

    carregarVendas();

    document.getElementById("produto").value = "";
    document.getElementById("quantidade").value = "";
    document.getElementById("data").value = "";
}

function excluirVenda(index) {
    let vendas = JSON.parse(localStorage.getItem("vendas")) || [];
    vendas.splice(index, 1);
    localStorage.setItem("vendas", JSON.stringify(vendas));
    carregarVendas();
}

// Auxiliar para formatar datas no padrão brasileiro
function formatarData(dataISO) {
    if (!dataISO) return "";
    const partes = dataISO.split("-");
    if (partes.length === 3) {
        return `${partes[2]}/${partes[1]}/${partes[0]}`;
    }
    return dataISO;
}
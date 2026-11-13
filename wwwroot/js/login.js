/* ==========================================================================
   LÓGICA JAVASCRIPT: TELA DE LOGIN
   Caminho: wwwroot/js/login.js
   ========================================================================== */

document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("formLogin");

    if (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();

            const usuario = document.getElementById("usuario").value;
            const senha = document.getElementById("senha").value;

            // Redireciona para a rota Razor Pages do ASP.NET Core (/Index em vez de home.html)
            if (usuario === "UniEletronics" && senha === "2026") {
                window.location.href = "/Index";
            } else {
                alert("ID ou senha incorretos!");
            }
        });
    }
});
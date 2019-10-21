var auth = {};

(function () {
    'use strict';

    auth = {
        login: function () {
            axios.post("/datacenter/authenticate", {
                UserName: $("#inputUser")[0].value,
                Password: $("#inputPassword")[0].value,
                TenantCode: $("#inputTenant")[0].value
            }).then(function (response) {
                $("#inputToken")[0].value = response.data;
                console.log("<h2>Success</h2>"
                    + " <div style='word-break:break-word; width: 600px;'>TOKEN:  "
                    + response.data + "</div>");
                document.querySelectorAll('a[role="tab"]')[1].click();
            }).catch(function (error) {
                const newWindow = window.open();
                newWindow.document.write(`<h2>Fail</h2><pre>${
                    JSON.stringify(error.response, null, 4)}</pre>`);
            });
        },
        download: function (filename) {
            const pom = document.createElement("a");
            pom.setAttribute("href", `files/${filename}`);
            pom.setAttribute("download", filename);
            $(document.body).append(pom);
            pom.click();
            return;

        },
        getFileName: function () {
            axios.get("/datacenter/file", {
                headers: { 'Authorization': `Bearer ${$("#inputToken")[0].value}` },
                params: {
                    Owner: $("#inputOwner")[0].value,
                    Plant: $("#inputPlant")[0].value,
                    TagNumber: $("#inputTag")[0].value,
                    ValveType: $("#inputValveType")[0].value,
                    FileType: $("#inputFileType")[0].value,
                    SerialNumber: $("#inputSerial")[0].value
                }
            }).then(function (response) {
                auth.download(response.data);
            }).catch(function (error) {
                const newWindow = window.open();
                newWindow.document.write(`<h2>Fail</h2>${
                    error.response.data ? error.response.data :
                        error.response.headers["www-authenticate"]}`);
            }).finally(function () {
            });
        }
    };
})();
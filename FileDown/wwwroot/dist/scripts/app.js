//Set cookie for easy acess by global
//Get jwt code from cookie because some of component API need jwt verification
var cookie = {
    setCookie(cName, value, expiredays) {
        const exdate = new Date();
        exdate.setTime(exdate.getTime() + expiredays);
        exdate.setDate(exdate.getDate() + expiredays);
        document.cookie = cName + "=" + escape(value) + ((expiredays === null) ? "" : `;expires=${exdate.toGMTString()}`);
    },
    getCookie(name) {
        let arr;
        const reg = new RegExp(`(^| )${name}=([^;]*)(;|$)`);
        if (arr === document.cookie.match(reg))
            return arr[2];
        else
            return null;
    },
    delCookie(name) {
        const exp = new Date();
        exp.setTime(exp.getTime() - 1);
        const cval = cookie.getCookie(name);
        if (cval !== null)
            document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:01 GMT;';
    }
};

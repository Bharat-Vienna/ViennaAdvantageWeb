/*
 * Globalize Culture en-AU
 *
 * http://github.com/jquery/globalize
 *
 * Copyright Software Freedom Conservancy, Inc.
 * Dual licensed under the MIT or GPL Version 2 licenses.
 * http://jquery.org/license
 *
 * This file was generated by the Globalize Culture Generator
 * Translation: bugs found in this file need to be fixed in the generator
 */

(function (window, undefined) {

    var Globalize;

    if (typeof require !== "undefined" &&
        typeof exports !== "undefined" &&
        typeof module !== "undefined") {
        // Assume CommonJS
        Globalize = require("globalize");
    } else {
        // Global variable
        Globalize = window.Globalize;
    }

    Globalize.addCultureInfo("en-AU", "default", {
        name: "en-AU",
        englishName: "English (Australia)",
        nativeName: "English (Australia)",
        numberFormat: {
            currency: {
                pattern: ["-$n", "$n"]
            }
        },
        calendars: {
            standard: {
                firstDay: 1,
                patterns: {
                    d: "d/MM/yyyy",
                    D: "dddd, d MMMM yyyy",
                    f: "dddd, d MMMM yyyy h:mm tt",
                    F: "dddd, d MMMM yyyy h:mm:ss tt",
                    M: "dd MMMM",
                    Y: "MMMM yyyy"
                }
            }
        },
        messages: {
            "Connection": "Connection",
            "Defaults": "Defaults",
            "Login": "Login",
            "File": "File",
            "Exit": "Exit",
            "Help": "Help",
            "About": "About",
            "Host": "Server",
            "Database": "Database",
            "User": "User ID",
            "EnterUser": "Enter Application User ID",
            "Password": "Password",
            "EnterPassword": "Enter Application Password",
            "Language": "Language",
            "SelectLanguage": "Select your language ",
            "Role": "Role",
            "Client": "Client",
            "Organization": "Organization",
            "Date": "Date",
            "Warehouse": "Warehouse",
            "Printer": "Printer",
            "Connected": "Connected",
            "NotConnected": "Not Connected",
            "DatabaseNotFound": "Database not found",
            "UserPwdError": "User does not match password",
            "RoleNotFound": "Role not found/complete",
            "Authorized": "Authorized",
            "Ok": "Ok",
            "Cancel": "Cancel",
            "VersionConflict": "Version Conflict:",
            "VersionInfo": "Server <> Client",
            "PleaseUpgrade": "Please download new Version from Server",
            "GoodMorning": "Good Morning",
            "GoodAfternoon": "Good Afternoon",
            "GoodEvening": "Good Evening",

            //New Resource

            "Back": "Back",
            "SelectRole": "Select Role",
            "SelectOrg": "Select Organization",
            "SelectClient": "Select Client",
            "SelectWarehouse": "Select Warehouse",
            "VerifyUserLanguage": "Verifying user and language",
            "LoadingPreference": "Loading Preference",
            "Completed": "Completed",
            //new
            "RememberMe": "Remember Me",
           
        }
    });

}(this));

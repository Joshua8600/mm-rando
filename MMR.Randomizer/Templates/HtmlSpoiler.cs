// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 16.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace MMR.Randomizer.Templates
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using MMR.Randomizer.Extensions;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class HtmlSpoiler : HtmlSpoilerBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("<html>\r\n<head>\r\n<style>\r\n    body.dark-mode {\r\n      background-color: #111;\r\n   " +
                    "   color: #ccc;\r\n    }\r\n    body.dark-mode a {\r\n      color: #111;\r\n    }\r\n    b" +
                    "ody.dark-mode button {\r\n      background-color: #ddd;\r\n      color: #111;\r\n    }" +
                    "\r\n\r\n    body.light-mode {\r\n      background-color: #eee;\r\n      color: #111;\r\n  " +
                    "  }\r\n    body.light-mode a {\r\n      color: #111;\r\n    }\r\n    body.light-mode but" +
                    "ton {\r\n      background-color: #111;\r\n      color: #ccc;\r\n    }\r\n\r\n    th{ text-" +
                    "align:left }\r\n    .region { text-align: center; font-weight: bold; }\r\n    [data-" +
                    "content]:before { content: attr(data-content); }\r\n\r\n    .dark-mode .spoiler{ bac" +
                    "kground-color:#ccc }\r\n    .dark-mode .spoiler:active { background-color: #111;  " +
                    "}\r\n    .dark-mode .show-highlight .unavailable .newlocation { background-color: " +
                    "#500705; }\r\n    .dark-mode .show-highlight .acquired .newlocation { background-c" +
                    "olor: #69591f; }\r\n    .dark-mode .show-highlight .available .newlocation { backg" +
                    "round-color: #313776; }\r\n\r\n    .light-mode .spoiler{ background-color:#111 }\r\n  " +
                    "  .light-mode .spoiler:active { background-color: #ccc;  }\r\n    .light-mode .sho" +
                    "w-highlight .unavailable .newlocation { background-color: #FF9999; }\r\n    .light" +
                    "-mode .show-highlight .acquired .newlocation { background-color: #99FF99; }\r\n   " +
                    " .light-mode .show-highlight .available .newlocation { background-color: #9999FF" +
                    "; }\r\n\r\n\r\n    #spoilerLogState { width: 560px; }\r\n\r\n    .invisible {\r\n        dis" +
                    "play: none;\r\n    }\r\n    .settingsFlex {\r\n        display: flex;\r\n        flex-di" +
                    "rection: row;\r\n    }\r\n    .settingsFlex span {\r\n        width: 100%;\r\n        te" +
                    "xt-align: center;\r\n        padding: 3px 0px;\r\n    }\r\n    #hideItemLabel {\r\n     " +
                    "   text-align: left;\r\n        display: inline;\r\n        width: auto;\r\n    }\r\n   " +
                    " .centeredLabel {\r\n        text-align: center;\r\n        display: inline-block;\r\n" +
                    "        width: 100%;\r\n    }\r\n    button {\r\n        padding: 4px 15px;\r\n        b" +
                    "order-radius: 8px;\r\n    }\r\n    @media (max-width: 860px) {\r\n        .settingsFle" +
                    "x {\r\n            flex-direction: column;\r\n        }\r\n        #hideItemLabel {\r\n " +
                    "           text-align: center;\r\n            display: inline-block;\r\n            " +
                    "width: 100%;\r\n        }\r\n    }\r\n\r\n</style>\r\n</head>\r\n<body class=\"light-mode\">\r\n" +
                    "<label><b>Version: </b></label><span>");
            this.Write(this.ToStringHelper.ToStringWithCulture(spoiler.Version));
            this.Write("</span><br/>\r\n<label><b>Settings: </b></label><code style=\"word-break: break-all;" +
                    "\" class=\"invisible\" id=\"settingsCode\">");
            this.Write(this.ToStringHelper.ToStringWithCulture(spoiler.SettingsString));
            this.Write("</code>\r\n<button type=\"button\" onclick=\"showHideSettings()\">Toggle Setting Displa" +
                    "y</button><br/><br/>\r\n<label><b>Seed: </b></label><span>");
            this.Write(this.ToStringHelper.ToStringWithCulture(spoiler.Seed));
            this.Write("</span><br/>\r\n<br/>\r\n<button type=\"button\" onclick=\"toggleDarkLight()\" title=\"Tog" +
                    "gle dark/light mode\">Toggle Dark Theme</button>\r\n<br/>\r\n<br/>\r\n<label><b>Spoiler" +
                    " Log State: </b></label><input id=\"spoilerLogState\" type=\"text\"/><br/>\r\n");
 if (spoiler.DungeonEntrances.Any()) { 

            this.Write("<h2>Dungeon Entrance Replacements</h2>\r\n<table border=\"1\" class=\"item-replacement" +
                    "s\">\r\n    <tr>\r\n        <th>Entrance</th>\r\n        <th></th>\r\n        <th>New Des" +
                    "tination</th>\r\n    </tr>\r\n");
         foreach (var kvp in spoiler.DungeonEntrances) {
            var entrance = kvp.Key;
            var destination = kvp.Value;
            this.Write("    <tr data-id=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture((int)destination));
            this.Write("\" data-newlocationid=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture((int)entrance));
            this.Write("\" class=\"unavailable\">\r\n        <td class=\"newlocation\">");
            this.Write(this.ToStringHelper.ToStringWithCulture(entrance.Entrance()));
            this.Write("</td>\r\n        <td><input type=\"checkbox\"/></td>\r\n        <td class=\"spoiler item" +
                    "name\"><span data-content=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(destination.Entrance()));
            this.Write("\"></span></td>\r\n    </tr>\r\n");
 } 
            this.Write("</table>\r\n");
 } 
            this.Write(@"<h2>Item Replacements</h2>
<label id=""hideItemLabel""><b>Hide Item Types:</b></label>
<span class=""settingsFlex"">
    <span>
        <input type=""checkbox"" id=""permanents"" onclick=""updateItemDisplaySettings()"" /> Permanents/Traps
    </span >
    <span>
        <input type=""checkbox"" id=""lowRupees"" onclick=""updateItemDisplaySettings()""/> Low Value Rupees</span>
    <span>
        <input type=""checkbox"" id=""highRupees"" onclick=""updateItemDisplaySettings()"" /> High Value Rupees
    </span><span><input type=""checkbox"" id=""hearts"" onclick=""updateItemDisplaySettings()"" /> Hearts</span>
    <span>
        <input type=""checkbox"" id=""bottleContents"" onclick=""updateItemDisplaySettings()"" /> Bottle Contents
    </span>
    <span>
        <input type=""checkbox"" id=""ammo"" onclick=""updateItemDisplaySettings()"" /> Ammo
    </span>
</span>
<br />
<span class=""centeredLabel""><button type=""button"" onclick=""updateItemDisplaySettings()"" />Refresh</button></span>
<br />
<br />
<input type=""checkbox"" id=""highlight-checks""/> Highlight available checks
<table border=""1"" class=""item-replacements"">
 <tr>
     <th>Location</th>
     <th></th>
     <th></th>
 </tr>
");
 foreach (var region in spoiler.ItemList.GroupBy(item => item.Region).OrderBy(g => g.Key)) {

            this.Write(" <tr class=\"region\"><td colspan=\"3\">");
            this.Write(this.ToStringHelper.ToStringWithCulture(region.Key.Name()));
            this.Write("</td></tr>\r\n ");
 foreach (var item in region.OrderBy(item => item.NewLocationName)) { 
            this.Write(" <tr data-id=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Id));
            this.Write("\" data-newlocationid=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.NewLocationId));
            this.Write("\" class=\"unavailable\">\r\n    <td class=\"newlocation\">");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.NewLocationName));
            this.Write("</td>\r\n    <td><input type=\"checkbox\"/></td>\r\n    <td class=\"spoiler itemname\"> <" +
                    "span data-content=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Name));
            this.Write("\"></span></td>\r\n </tr>\r\n ");
 } 
 } 
            this.Write("</table>\r\n<h2>Item Locations</h2>\r\n<table border=\"1\" id=\"item-locations\">\r\n <tr>\r" +
                    "\n     <th>Item</th>\r\n     <th></th>\r\n     <th>Location</th>\r\n </tr>\r\n");
 foreach (var item in spoiler.ItemList.Where(item => !item.IsJunk)) {

            this.Write(" <tr data-id=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Id));
            this.Write("\" data-newlocationid=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.NewLocationId));
            this.Write("\">\r\n    <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Name));
            this.Write("</td>\r\n    <td><input type=\"checkbox\"/></td>\r\n    <td class=\"spoiler newlocation\"" +
                    "> <span data-content=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.NewLocationName));
            this.Write("\"></span></td>\r\n </tr>\r\n");
 } 
            this.Write("</table>\r\n");
 if (spoiler.GossipHints != null && spoiler.GossipHints.Any()) { 

            this.Write("<h2>Gossip Stone Hints</h2>\r\n<table border=\"1\">\r\n    <tr>\r\n        <th>Gossip Sto" +
                    "ne</th>\r\n        <th>Message</th>\r\n    </tr>\r\n");
    foreach (var hint in spoiler.GossipHints.OrderBy(h => h.Key.ToString())) { 

            this.Write("    <tr>\r\n        <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(hint.Key));
            this.Write("</td>\r\n        <td class=\"spoiler\"><span data-content=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(hint.Value));
            this.Write("\"></span></td>\r\n    </tr>\r\n");
 } 
            this.Write("</table>\r\n");
 } 
            this.Write("<script>\r\n    function all(list, predicate) {\r\n        for (var i = 0; i < list.l" +
                    "ength; i++) {\r\n            if (!predicate(list[i])) {\r\n                return fa" +
                    "lse;\r\n            }\r\n        }\r\n        return true;\r\n    }\r\n\r\n    function any(" +
                    "list, predicate) {\r\n        for (var i = 0; i < list.length; i++) {\r\n           " +
                    " if (predicate(list[i])) {\r\n                return true;\r\n            }\r\n       " +
                    " }\r\n        return false;\r\n    }\r\n\r\n    function includes(list, item) {\r\n       " +
                    " for (var i = 0; i < list.length; i++) {\r\n            if (list[i] === item) {\r\n " +
                    "               return true;\r\n            }\r\n        }\r\n        return false;\r\n  " +
                    "  }\r\n    \r\n    var segmentSize = 16;\r\n    function saveItems() {\r\n        var se" +
                    "gments = [];\r\n        for (var i = 0; i < logic.length; i++) {\r\n            var " +
                    "segmentIndex = parseInt(i / segmentSize);\r\n            segments[segmentIndex] = " +
                    "segments[segmentIndex] || 0;\r\n            if (logic[i].Checked) {\r\n             " +
                    "   segments[parseInt(i / segmentSize)] += (1 << (i%segmentSize));\r\n            }" +
                    "\r\n        }\r\n        var saveString = segments.map(function(s) {\r\n            re" +
                    "turn s.toString(16);\r\n        }).join(\"-\");\r\n        var saveInput = document.qu" +
                    "erySelector(\"#spoilerLogState\");\r\n        saveInput.value = saveString;\r\n    }\r\n" +
                    "\r\n    function loadItems() {\r\n        var saveInput = document.querySelector(\"#s" +
                    "poilerLogState\");\r\n        var segments = saveInput.value.split(\"-\");\r\n        i" +
                    "f (Math.ceil((logic.length - 1) / segmentSize) !== segments.length) {\r\n         " +
                    "   alert(\"Invalid Spoiler Log state\");\r\n            return;\r\n        }\r\n        " +
                    "segments = segments.map(function(segment) {\r\n            return parseInt(segment" +
                    ", 16);\r\n        });\r\n        var itemsToCheck = [];\r\n        for (var i = 0; i <" +
                    " segments.length; i++) {\r\n            var segment = segments[i];\r\n            fo" +
                    "r (var j = 0; j < segmentSize; j++) {\r\n                var itemIndex = segmentSi" +
                    "ze * i + j;\r\n                if (itemIndex < logic.length) {\r\n                  " +
                    "  var mark = ((segment >> j) % 2 == 1);\r\n                    logic[itemIndex].Ch" +
                    "ecked = mark;\r\n                    var itemRow = document.querySelector(\"tr[data" +
                    "-newlocationid=\'\" + itemIndex + \"\']\");\r\n                    if (itemRow) {\r\n    " +
                    "                    logic[itemRow.dataset.id].Acquired = mark;\r\n                " +
                    "        if (!includes(itemsToCheck, itemRow.dataset.id)) {\r\n                    " +
                    "        itemsToCheck.push(itemRow.dataset.id);\r\n                        }\r\n     " +
                    "               } else {\r\n                        logic[itemIndex].Acquired = mar" +
                    "k;\r\n                        if (!includes(itemsToCheck, itemIndex)) {\r\n         " +
                    "                   itemsToCheck.push(itemIndex);\r\n                        }\r\n   " +
                    "                 }\r\n                }\r\n            }\r\n        }\r\n        checkIt" +
                    "ems(itemsToCheck);\r\n    }\r\n\r\n    document.querySelector(\"#spoilerLogState\").addE" +
                    "ventListener(\"keypress\", function(event) {\r\n        if (event.keyCode === 13) {\r" +
                    "\n            loadItems();\r\n        }\r\n    });\r\n\r\n    function checkLocations(loc" +
                    "ations) {\r\n        var itemsToCheck = [];\r\n        for (var i = 0; i < locations" +
                    ".length; i++) {\r\n            var location = logic[locations[i]];\r\n            lo" +
                    "cation.IsAvailable = \r\n                (location.RequiredItemIds === null || loc" +
                    "ation.RequiredItemIds.length === 0 || all(location.RequiredItemIds, function(id)" +
                    " { return logic[id].Acquired; }))\r\n                && \r\n                (locatio" +
                    "n.ConditionalItemIds === null || location.ConditionalItemIds.length === 0 || any" +
                    "(location.ConditionalItemIds, function(conditionals) { return all(conditionals, " +
                    "function(id) { return logic[id].Acquired; }); }));\r\n            \r\n            if" +
                    " (!location.Acquired && location.IsFakeItem && location.IsAvailable) {\r\n        " +
                    "        location.Acquired = true;\r\n                itemsToCheck.push(locations[i" +
                    "]);\r\n            }\r\n            if (location.Acquired && location.IsFakeItem && " +
                    "!location.IsAvailable) {\r\n                location.Acquired = false;\r\n          " +
                    "      itemsToCheck.push(locations[i]);\r\n            }\r\n        \r\n            var" +
                    " locationRow = document.querySelector(\".item-replacements tr[data-newlocationid=" +
                    "\'\" + locations[i] + \"\']\");\r\n            if (locationRow) {\r\n                loca" +
                    "tionRow.className = \"\";\r\n                locationRow.classList.add(location.IsAv" +
                    "ailable ? \"available\" : \"unavailable\");\r\n                var itemName = location" +
                    "Row.querySelector(\".itemname\");\r\n                var checkbox = locationRow.quer" +
                    "ySelector(\"input\");\r\n                checkbox.checked = location.Checked;\r\n     " +
                    "           if (location.Checked) {\r\n                    itemName.classList.remov" +
                    "e(\"spoiler\");\r\n                } else {\r\n                    itemName.classList." +
                    "add(\"spoiler\");\r\n                }\r\n            }\r\n        \r\n            var ite" +
                    "mRow = document.querySelector(\"#item-locations tr[data-newlocationid=\'\" + locati" +
                    "ons[i] + \"\']\");\r\n            if (itemRow) {\r\n                var itemName = item" +
                    "Row.querySelector(\".newlocation\");\r\n                var checkbox = itemRow.query" +
                    "Selector(\"input\");\r\n                var item = logic[itemRow.dataset.id];\r\n     " +
                    "           checkbox.checked = item.Acquired;\r\n                if (item.Acquired)" +
                    " {\r\n                    itemName.classList.remove(\"spoiler\");\r\n                }" +
                    " else {\r\n                    itemName.classList.add(\"spoiler\");\r\n               " +
                    " }\r\n            }\r\n        }\r\n        if (itemsToCheck.length > 0) {\r\n          " +
                    "  checkItems(itemsToCheck);\r\n        } else {\r\n            saveItems();\r\n       " +
                    " }\r\n    }\r\n\r\n    var logic = ");
            this.Write(this.ToStringHelper.ToStringWithCulture(spoiler.LogicJson));
            this.Write(";\r\n\r\n    for (var i = 0; i < logic.length; i++) {\r\n        var item = logic[i];\r\n" +
                    "        if (item.Acquired) {\r\n            item.Checked = true;\r\n            docu" +
                    "ment.querySelector(\"tr[data-newlocationid=\'\" + i + \"\'] input\").checked = true;\r\n" +
                    "        }\r\n        if (item.RequiredItemIds !== null) {\r\n            for (var j " +
                    "= 0; j < item.RequiredItemIds.length; j++) {\r\n                var id = item.Requ" +
                    "iredItemIds[j];\r\n                if (!logic[id].LocksLocations) {\r\n             " +
                    "       logic[id].LocksLocations = [];\r\n                }\r\n                if (!i" +
                    "ncludes(logic[id].LocksLocations, i)) {\r\n                    logic[id].LocksLoca" +
                    "tions.push(i);\r\n                }\r\n            }\r\n        }\r\n        if (item.Co" +
                    "nditionalItemIds !== null) {\r\n            for (var k = 0; k < item.ConditionalIt" +
                    "emIds.length; k++) {\r\n                for (var j = 0; j < item.ConditionalItemId" +
                    "s[k].length; j++) {\r\n                    var id = item.ConditionalItemIds[k][j];" +
                    "\r\n                    if (!logic[id].LocksLocations) {\r\n                        " +
                    "logic[id].LocksLocations = [];\r\n                    }\r\n                    if (!" +
                    "includes(logic[id].LocksLocations, i)) {\r\n                        logic[id].Lock" +
                    "sLocations.push(i);\r\n                    }\r\n                }\r\n            }\r\n  " +
                    "      }\r\n    }\r\n\r\n    function checkItems(itemIds) {\r\n        var locationsToChe" +
                    "ck = [];\r\n        for (var i = 0; i < itemIds.length; i++) {\r\n            var it" +
                    "emId = itemIds[i];\r\n            if (logic[itemId].LocksLocations) {\r\n           " +
                    "     for (var j = 0; j < logic[itemId].LocksLocations.length; j++) {\r\n          " +
                    "          var locationId = logic[itemId].LocksLocations[j];\r\n                   " +
                    " if (!includes(locationsToCheck, locationId)) {\r\n                        locatio" +
                    "nsToCheck.push(locationId);\r\n                    }\r\n                }\r\n         " +
                    "   }\r\n        }\r\n        checkLocations(locationsToCheck);\r\n    }\r\n\r\n    var sta" +
                    "rtingLocations = [0, 94, 274, 275, 276, 277];\r\n    for (var id of startingLocati" +
                    "ons) {\r\n        logic[id].Checked = true;\r\n        var itemId = document.querySe" +
                    "lector(\"tr[data-newlocationid=\'\" + id + \"\']\").dataset.id;\r\n        logic[itemId]" +
                    ".Acquired = true;\r\n        document.querySelector(\"tr[data-newlocationid=\'\" + id" +
                    " + \"\'] input\").checked = true;\r\n    }\r\n\r\n    var allLocationIds = [];\r\n    for (" +
                    "var i = 0; i < logic.length; i++) {\r\n        allLocationIds.push(i);\r\n    }\r\n   " +
                    " checkLocations(allLocationIds);\r\n\r\n    var rows = document.querySelectorAll(\"tr" +
                    "\");\r\n    for (var i = 1; i < rows.length; i++) {\r\n        var row = rows[i];\r\n  " +
                    "      var checkbox = row.querySelector(\"input\");\r\n        if (checkbox) {\r\n     " +
                    "       checkbox.addEventListener(\"click\", function(e) {\r\n                var row" +
                    " = e.target.closest(\"tr\");\r\n                var rowId = parseInt(row.dataset.id)" +
                    ";\r\n                var newLocationId = parseInt(row.dataset.newlocationid);\r\n   " +
                    "             logic[newLocationId].Checked = e.target.checked;\r\n                l" +
                    "ogic[rowId].Acquired = e.target.checked;\r\n                checkLocations([newLoc" +
                    "ationId]);\r\n                checkItems([rowId]);\r\n            });\r\n        }\r\n  " +
                    "  }\r\n\r\n    document.querySelector(\"#highlight-checks\").addEventListener(\"click\"," +
                    " function(e) {\r\n        var tables = document.querySelectorAll(\"table.item-repla" +
                    "cements\");\r\n        for (var i = 0; i < tables.length; i++) {\r\n            if (e" +
                    ".target.checked) {\r\n                tables[i].classList.add(\"show-highlight\");\r\n" +
                    "            } else {\r\n                tables[i].classList.remove(\"show-highlight" +
                    "\");\r\n            }\r\n        }\r\n    });\r\n\r\n    function toggleDarkLight() {\r\n    " +
                    "    var body = document.getElementsByTagName(\'body\')[0];\r\n        var currentCla" +
                    "ssBody = body.className;\r\n        body.className = currentClassBody === \"dark-mo" +
                    "de\" ? \"light-mode\" : \"dark-mode\";\r\n    }\r\n\r\n    function showHideSettings() {\r\n " +
                    "       var settingsCode = document.getElementById(\"settingsCode\");\r\n        var " +
                    "currentClassSettingsCode = settingsCode.className;\r\n        settingsCode.classNa" +
                    "me = currentClassSettingsCode === \"visible\" ? \"invisible\" : \"visible\";\r\n    }\r\n " +
                    "   removableSettings = {};\r\n    removableSettings[\"hearts\"] = [\"Piece of Heart\"," +
                    " \"Heart Container\"];\r\n    removableSettings[\"lowRupees\"] = [\"Blue Rupee\", \"Red R" +
                    "upee\"];\r\n    removableSettings[\"highRupees\"] = [\"Purple Rupee\", \"Silver Rupee\", " +
                    "\"Gold Rupee\"];\r\n    removableSettings[\"bottleContents\"] = [\"Bottle:\", \"Milk\", \"C" +
                    "hateau\", \"Potion\"];\r\n    removableSettings[\"permanents\"] = [\"Mask\", \"Stray Fairy" +
                    "\", \"Skulltula Spirit\", \"Song\", \"Sonata\", \"Lullaby\", \"Bossa\", \"Elegy\", \"Oath to O" +
                    "rder\", \"Shield\", \"Upgrade\", \"Pictobox\", \"Lens of Truth\", \"Hookshot\", \"Spin Attac" +
                    "k\", \"Double Defense\", \"Sword\", \"Notebook\", \"Hat\", \"Hood\", \"Map\", \"Ice Trap\", \"Em" +
                    "pty Bottle\", \"Compass\"];\r\n    removableSettings[\"ammo\"] = [\"Arrow\", \"Bombs\", \"Bo" +
                    "mbchu\", \"Deku Nuts\", \"Deku Stick\"];\r\n    function updateItemDisplaySettings() {\r" +
                    "\n        var listOfItems = document.querySelectorAll(\'.itemname span\');\r\n       " +
                    " for (const [key, value] of Object.entries(removableSettings)) {\r\n            li" +
                    "stOfStuffToRemove = removableSettings[key];\r\n            for (i = 0; i < listOfI" +
                    "tems.length; i = i + 1) {\r\n                theItem = listOfItems[i];\r\n          " +
                    "      if (!theItem.parentNode.getAttribute(\"class\").match(\"spoiler\")) {\r\n       " +
                    "             for (j = 0; j < listOfStuffToRemove.length; j = j + 1) {\r\n         " +
                    "               if (theItem.getAttribute(\"data-content\").match(listOfStuffToRemov" +
                    "e[j])) {\r\n                            if (document.getElementById(key).checked) " +
                    "{\r\n                                theItem.parentNode.parentNode.style.display =" +
                    " \"none\";\r\n                                break;\r\n                            } " +
                    "else {\r\n                                theItem.parentNode.parentNode.style.disp" +
                    "lay = \"table-row\";\r\n                            }\r\n                        }\r\n  " +
                    "                  }\r\n                }\r\n            }\r\n        }\r\n    }\r\n</scrip" +
                    "t>\r\n</body>\r\n</html>\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public class HtmlSpoilerBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}

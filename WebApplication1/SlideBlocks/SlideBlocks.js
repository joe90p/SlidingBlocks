$(document).ready(start);

function start() {
    var tiles = document.getElementById("board").getElementsByTagName("div");
    var blankTile = document.getElementById("blank");
    var solveButton = document.getElementById("solve");

    for (var i = 0, len = tiles.length; i < len; ++i) {
        tiles[i].addEventListener("click", tile_Click);
    }

    solveButton.addEventListener("click", solve_Click);

    function getOrderedTileArray() {
        var tileArray = [];

        for (var i = 0, len = tiles.length; i < len; ++i) {
            var p = getPoint(tiles[i]);
            tileArray.push({ value: tiles[i], key: (p.row * 10) + (p.col * 1) });
        }
        tileArray.sort(function(a, b) {
            return a.key - b.key;
        });
        return tileArray.map(function(t) { return t.value; });
    }

    function getInnerTextConcatString(domElementArray) {
        var concatString = "";
        domElementArray.map(function (val) {
            concatString += val.innerText === "" ? "0" : val.innerText;
        });
        return concatString;
    }
    
    function solve_Click() {

        var tileArray = getOrderedTileArray();
        var tilesStateAsString = getInnerTextConcatString(tileArray);
        
        $.ajax({
            dataType: "json",
            url: "http://localhost/TestService/Service1.svc/GetSwaps",
            data: { boardConfig: tilesStateAsString },
            success: success,
            error: fail
        });
        
        function swapTiles(swapsArray, i) {
            var swap = swapsArray[swapsArray.length - 1 - i];
            swapTileGridPositions(tileArray[swap.s1], tileArray[swap.s2]);
            tileArray = getOrderedTileArray();
        }
        
        function success(data, textStatus, jqXHR) {
            asyncLoop(data, function () {
                alert('solved');
            }, swapTiles);
        }
        
        function fail(jqXHR,  textStatus,  errorThrown ){
            alert('fail');

        }  
        
    } 
    
    function tile_Click()
    {
        var tilePosition = getPoint(this);
        var blankTilePosition = getPoint(blankTile);
        if (validMove(tilePosition, blankTilePosition))
        {
            setGridPosition(this, blankTilePosition);
            setGridPosition(blankTile, tilePosition);
        } 
    }
    
    function swapTileGridPositions(tile1, tile2) {
        var t1Position = getPoint(tile1);
        var t2Position = getPoint(tile2);
        setGridPosition(tile1, t2Position);
        setGridPosition(tile2, t1Position);
    }
    
    function setGridPosition(domElement, position) {
        domElement.style.setProperty('-ms-grid-column', position.col);
        domElement.style.setProperty('-ms-grid-row', position.row);
    }
        
    function getPoint(domElement) {
        var style = window.getComputedStyle(domElement);
        var tile = { col: style.getPropertyValue('-ms-grid-column'), row: style.getPropertyValue('-ms-grid-row') };
        return tile;
    }
        
    function validMove(tilePosition, blankTilePosition) {
        var colChange = absoluteValue(tilePosition.col - blankTilePosition.col);
        var rowChange = absoluteValue(tilePosition.row - blankTilePosition.row);
        return (colChange > 0 || rowChange > 0) && !(colChange === 1 && rowChange === 1) && colChange<2 && rowChange<2;
    }
    
    function asyncLoop(data, callback, useIndex) {
        (function loop(i) {
            useIndex(data, i);
            if (i < data.length - 1) {
                setTimeout(function () { loop(++i); }, 1000);
            } else {
                callback();
            }
        }(0));
    }
        
    function absoluteValue(number) {
        if (number < 0) {
            return number * -1;
        } else {
            return number;
        }
    }
}
// Example #1: clojures
let a = "global";
{
    function showA() {
        console.log(a);
    }
    console.log("Example #1: clojures");
    showA();
    let a = "block";
    showA();
    console.log("");
}
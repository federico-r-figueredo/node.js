function fibonacci(number) {
    if (number <= 1) return number;
    return fibonacci(number - 2) + fibonacci(number - 1);
}

let initialTime = Date.now();
for (let i = 0; i < 20; i = i + 1) {
    console.log(fibonacci(i));
}
let finalTime = Date.now();
let totalTime = finalTime - initialTime;

console.log("Total time: " + totalTime + " seconds");
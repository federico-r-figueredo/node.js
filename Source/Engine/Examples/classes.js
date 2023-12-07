// Example #1: instance methods
class Person {
    eat() {
        console.log("Crunch crunch crunch!");
    }
}
let person = new Person();
console.log("Example #1: instance methods");
person.eat();
console.log("");

// Example #2: handles to methods
class Egoist {
    speak() {
        console.log(this);
    }
}

let method = new Egoist().speak;
console.log("Example #2: handles to methods");
method();
console.log("");

// Example #3: late binding
class Cake {
    taste() {
        let adjective = "delicious";
        console.log("The " + this.flavor + " cake is " + adjective + "!");
    }
}

let cake = new Cake();
cake.flavor = "German chocolate";
console.log("Example #3: late binding");
cake.taste();
console.log("");

// Example #4: first order functions
class Thing {
    getCallback() {
        function localFunction() {
            console.log(this);
        }

        return localFunction;
    }
}

let callback = new Thing().getCallback();
console.log("Example #4: first order functions");
callback();
console.log("");

// Example #5: report error on 'this' use outside of a class
// console.log(this);

// Example #6: initializing object w/ constructor
class SuperHero {
    constructor(firstName, lastName) {
        this.firstName = firstName;
        this.lastName = lastName;
    }

    greet() {
        console.log("Hello! I'm " + this.firstName + " " + this.lastName);
    }
}

let superman = new SuperHero("Clark", "Kent");
console.log("Example #6");
superman.greet();
console.log("");


// Example #7: report error on direct 'constructor' call (on an instance)
// class User {
//     constructor(firstName, lastName) {
//         this.firstName = firstName;
//         this.lastName = lastName;
//     }
// }
// let fede = new User("Federico", "Figueredo");
// fede = fede.constructor();

// Example #8: report error on 'return' statement inside a constructor
// class Employee {
//     constructor(firstName, lastName) {
//         this.firstName = firstName;
//         this.lastName = lastName;

//         // return 1;
//         return;
//     }
// }

// let ceci = new User("Cecilia", "Bianchi");

// Example #9: class based inheritance
class DoughnutA {
    cook() {
        console.log("Fry until golden brown.");
    }
}

class BostonCreamA extends DoughnutA {}

let meal = new BostonCreamA();
console.log("Example #10: class based inheritance");
meal.cook();
console.log("");

class DoughnutB {
    cook() {
        console.log("Fry until golden brown.");
    }
}

// Example #10: super keyword
class BostonCreamB extends DoughnutB {
    cook() {
        super.cook();
        console.log("Pipe full of custard and coat with chocolate.");
    }
}
console.log("Example #11: super keyword");
BostonCreamB().cook();
console.log("");

// Example #11: super keyword bound to superclass of object that declared (not the one executing it)
class A {
    method() {
        console.log("Method A");
    }
}

class B extends A {
    method() {
        console.log("Method B");
    }

    test() {
        super.method();
    }
}

class C extends B {}
console.log("Example #12: super keyword binding");
C().test();
console.log("");
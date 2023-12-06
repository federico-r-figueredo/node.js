// Error: Can't use 'super' in a class that isn't extending a superclass.
class Eclair {
    cook() {
        super.cook();
        console.log("Pipe full of crème pâtissière.");
    }
}

// Error: Can't use 'super' outside fo a class.
//super.notEvenInAClass();
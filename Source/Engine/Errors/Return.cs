using System;

namespace NodeJS.Engine.Errors {
    internal class Return : SystemException {
        private readonly object value;

        public Return(object value) : base() {
            this.value = value;
        }

        public object Value => value;
    }
}
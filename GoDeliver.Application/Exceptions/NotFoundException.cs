﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDeliver.Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string entity, object key)
            : base($"{entity} with key '{key}' was not found.") { }
    }
}

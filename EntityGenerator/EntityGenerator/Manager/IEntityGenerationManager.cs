﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using EntityGenerator.Contracts;

namespace EntityGenerator.Manager
{
	public interface IEntityGenerationManager
	{
		Task GenerateEntities( EntityGenerationRequest request );
	}
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Level14.BoardGameWeb.Models
{
    public class BoardContext: DbContext
    {
        public DbSet<Session> Sessions { get; set; }
    }
}
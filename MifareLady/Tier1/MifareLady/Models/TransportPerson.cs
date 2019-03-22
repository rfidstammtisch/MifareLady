using BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MifareLady.Models
{
    public class TransportPerson : TransportDataset
    {
        public override string Type { get => "Person"; }
        public string PersonNumber { get => (dataset as Person).PersonNumber; set => (dataset as Person).PersonNumber = value; }
        public string FirstName { get => (dataset as Person).FirstName; set => (dataset as Person).FirstName = value; }
        public string LastName { get => (dataset as Person).LastName; set => (dataset as Person).LastName = value; }
        public string Title { get => (dataset as Person).Title; set => (dataset as Person).Title = value; }
        public bool IsBlocked { get => (dataset as Person).IsBlocked; set => (dataset as Person).IsBlocked = value; }
        public byte[] Image { get => (dataset as Person).Image; set => (dataset as Person).Image = value; }

        public TransportPerson() => dataset = new Person();
        public TransportPerson(Person person) => this.dataset = person;
    }
}
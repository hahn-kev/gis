namespace Backend.Entities
{
    public class Grade : BaseEntity
    {
        public int GradeNo { get; set; }
        public decimal MinPay { get; set; }
        public decimal MaxPay { get; set; }
    }
}
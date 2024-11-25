//using system;
//using system.collections.generic;
//using microsoft.entityframeworkcore;
//using webapplication1.models;

//namespace webapplication1.data;

//public partial class scmdbcontext : dbcontext
//{
//    public scmdbcontext()
//    {
//    }

//    public scmdbcontext(dbcontextoptions<scmdbcontext> options)
//        : base(options)
//    {
//    }

//    public virtual dbset<usercredential> usercredentials { get; set; }

//    protected override void onconfiguring(dbcontextoptionsbuilder optionsbuilder)
//#warning to protect potentially sensitive information in your connection string, you should move it out of source code. you can avoid scaffolding the connection string by using the name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. for more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?linkid=723263.
//        => optionsbuilder.usesqlserver("data source=(localdb)\\mssqllocaldb;initial catalog=scmbuildsite;integrated security=true;connect timeout=30;encrypt=false;trust server certificate=false;application intent=readwrite;multi subnet failover=false");

//    protected override void onmodelcreating(modelbuilder modelbuilder)
//    {
//        modelbuilder.entity<usercredential>(entity =>
//        {
//            entity.haskey(e => e.userid).hasname("pk__usercred__1788cc4ce07f1310");

//            entity.property(e => e.email).hasmaxlength(100);
//            entity.property(e => e.passwordhash).hasmaxlength(100);
//            entity.property(e => e.username).hasmaxlength(50);
//        });

//        onmodelcreatingpartial(modelbuilder);
//    }

//    partial void onmodelcreatingpartial(modelbuilder modelbuilder);
//}

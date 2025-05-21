using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRelacionVotoOpcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    password = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "miembro"),
                    estado = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "activo"),
                    fecha_registro = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    ultimo_acceso = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuario__3213E83F20EA9486", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Asamblea",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titulo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    creador_id = table.Column<int>(type: "int", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValue: "programada"),
                    acta = table.Column<string>(type: "text", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Asamblea__3213E83FB30A2E56", x => x.id);
                    table.ForeignKey(
                        name: "FK__Asamblea__creado__14270015",
                        column: x => x.creador_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    contenido = table.Column<string>(type: "text", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    fecha_envio = table.Column<DateTime>(type: "datetime", nullable: true),
                    fecha_lectura = table.Column<DateTime>(type: "datetime", nullable: true),
                    estado = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValue: "pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__3213E83FCCDAF6FE", x => x.id);
                    table.ForeignKey(
                        name: "FK__Notificac__usuar__282DF8C2",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Reporte",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tipo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    fecha_generacion = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    generado_por = table.Column<int>(type: "int", nullable: false),
                    url_archivo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    formato = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Reporte__3213E83F61D7FB49", x => x.id);
                    table.ForeignKey(
                        name: "FK__Reporte__generad__2BFE89A6",
                        column: x => x.generado_por,
                        principalTable: "Usuario",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Restriccion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    tipo_restriccion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime", nullable: true),
                    motivo = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    creado_por = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Restricc__3213E83F71BA252E", x => x.id);
                    table.ForeignKey(
                        name: "FK__Restricci__cread__0F624AF8",
                        column: x => x.creado_por,
                        principalTable: "Usuario",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__Restricci__usuar__0E6E26BF",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    asamblea_id = table.Column<int>(type: "int", nullable: false),
                    titulo = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "datetime", nullable: false),
                    quorum_requerido = table.Column<decimal>(type: "decimal(5,4)", nullable: true, defaultValue: 5m),
                    estado = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true, defaultValue: "pendiente"),
                    tipo_votacion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Votacion__3213E83F0663BCAC", x => x.id);
                    table.ForeignKey(
                        name: "FK__Votacion__asambl__18EBB532",
                        column: x => x.asamblea_id,
                        principalTable: "Asamblea",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpcionVotacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    votacion_id = table.Column<int>(type: "int", nullable: false),
                    texto = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    orden = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OpcionVo__3213E83F25C6E54F", x => x.id);
                    table.ForeignKey(
                        name: "FK__OpcionVot__votac__1CBC4616",
                        column: x => x.votacion_id,
                        principalTable: "Votacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Voto",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    votacion_id = table.Column<int>(type: "int", nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false),
                    opcion_id = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    valor_ponderado = table.Column<decimal>(type: "decimal(10,4)", nullable: true, defaultValue: 10000m),
                    ip_origen = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Voto__3213E83FD7FC7F56", x => x.id);
                    table.ForeignKey(
                        name: "FK__Voto__usuario_id__236943A5",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK__Voto__votacion_i__22751F6C",
                        column: x => x.votacion_id,
                        principalTable: "Votacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_estado",
                table: "Asamblea",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "idx_fecha",
                table: "Asamblea",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Asamblea_creador_id",
                table: "Asamblea",
                column: "creador_id");

            migrationBuilder.CreateIndex(
                name: "idx_estado_notificacion",
                table: "Notificacion",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "idx_fechas_notificacion",
                table: "Notificacion",
                columns: new[] { "fecha_creacion", "fecha_envio", "fecha_lectura" });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacion_usuario_id",
                table: "Notificacion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "idx_orden",
                table: "OpcionVotacion",
                column: "orden");

            migrationBuilder.CreateIndex(
                name: "IX_OpcionVotacion_votacion_id",
                table: "OpcionVotacion",
                column: "votacion_id");

            migrationBuilder.CreateIndex(
                name: "idx_fecha_reporte",
                table: "Reporte",
                column: "fecha_generacion");

            migrationBuilder.CreateIndex(
                name: "idx_tipo",
                table: "Reporte",
                column: "tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Reporte_generado_por",
                table: "Reporte",
                column: "generado_por");

            migrationBuilder.CreateIndex(
                name: "idx_fechas",
                table: "Restriccion",
                columns: new[] { "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "idx_usuario",
                table: "Restriccion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Restriccion_creado_por",
                table: "Restriccion",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "idx_email",
                table: "Usuario",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_estado_usuario",
                table: "Usuario",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuario__AB6E616447B8B9FE",
                table: "Usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_estado_votacion",
                table: "Votacion",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "idx_fechas_votacion",
                table: "Votacion",
                columns: new[] { "fecha_inicio", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_Votacion_asamblea_id",
                table: "Votacion",
                column: "asamblea_id");

            migrationBuilder.CreateIndex(
                name: "idx_fecha_voto",
                table: "Voto",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Voto_usuario_id",
                table: "Voto",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "uk_voto_usuario",
                table: "Voto",
                columns: new[] { "votacion_id", "usuario_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacion");

            migrationBuilder.DropTable(
                name: "OpcionVotacion");

            migrationBuilder.DropTable(
                name: "Reporte");

            migrationBuilder.DropTable(
                name: "Restriccion");

            migrationBuilder.DropTable(
                name: "Voto");

            migrationBuilder.DropTable(
                name: "Votacion");

            migrationBuilder.DropTable(
                name: "Asamblea");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}

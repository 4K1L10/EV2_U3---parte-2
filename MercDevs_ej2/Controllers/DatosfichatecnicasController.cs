﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MercDevs_ej2.Models;
using Rotativa;
using Rotativa.AspNetCore;
using System.Net.Mail;
using System.Net;
using Microsoft.Build.Framework;
using System.Configuration;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MercDevs_ej2.Controllers
{
    public class DatosfichatecnicasController : Controller
    {
        private readonly MercydevsEjercicio2Context _context;
		private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public DatosfichatecnicasController(MercydevsEjercicio2Context context, EmailService emailService, IConfiguration configuration)
        {
            _context = context;
			_emailService = emailService;
            _configuration = configuration;
        }

       


        public async Task<IActionResult> FichaTecnica(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fichaTecnica = await _context.Datosfichatecnicas
                .Where(d => d.IdDatosFichaTecnica == id)
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.Diagnosticosolucions)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync();

            if (fichaTecnica == null)
            {
                return NotFound();
            }

            return View(fichaTecnica);  
        }

        public async Task<IActionResult> ExportarPDF(int id)
        {
            var fichaTecnica = await _context.Datosfichatecnicas
                .Where(d => d.IdDatosFichaTecnica == id)
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.Diagnosticosolucions)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync();

            if (fichaTecnica == null)
            {
                return NotFound();
            }

            var pdf = new ViewAsPdf("FichaTecnica", fichaTecnica)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
            };

            return pdf;
        }


        public IActionResult HeaderPDF()
        {
            return PartialView("HeaderPDF");
        }

        public IActionResult FooterPDF()
        {
            return PartialView("FooterPDF");
        }

        [HttpPost]
        public async Task<IActionResult> EnviarPDFPorCorreo([FromBody] EmailSettings request)
        {
            var fichaTecnica = await _context.Datosfichatecnicas
                .Where(d => d.IdDatosFichaTecnica == request.Id)
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.Diagnosticosolucions)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync();

            if (fichaTecnica == null)
            {
                return Json(new { success = false, message = "Ficha técnica no encontrada." });
            }

            var cliente = fichaTecnica.RecepcionEquipo.IdClienteNavigation;

            if (cliente == null || string.IsNullOrEmpty(cliente.Correo))
            {
                return Json(new { success = false, message = "No hay un cliente asociado a esta ficha técnica o el cliente no tiene un correo válido." });
            }

            // Generar el PDF
            var pdf = new ViewAsPdf("FichaTecnica", fichaTecnica)
            {
                CustomSwitches = "--footer-center \"[page]\" --footer-line --footer-font-size \"12\" --footer-spacing 10 --footer-font-name \"calibri\"",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
            };

            byte[] pdfBytes;
            try
            {
                pdfBytes = await pdf.BuildFile(ControllerContext);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al generar el PDF: {ex.Message}" });
            }

            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return Json(new { success = false, message = "El archivo PDF generado está vacío o es nulo." });
            }

            // Enviar el correo
            try
            {
                await _emailService.SendEmailAsync(
                    cliente.Correo,
                    "Ficha Técnica",
                    "Adjunto encontrarás la ficha técnica en formato PDF.",
                    pdfBytes,
                    "FichaTecnica.pdf"
                );

                return Json(new { success = true, message = "Correo enviado con éxito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al enviar el correo: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Inicio()
        {
            var mercydevsEjercicio2Context = _context.Datosfichatecnicas.Include(d => d.RecepcionEquipo);
            return View(await mercydevsEjercicio2Context.ToListAsync());
        }

        // GET: Datosfichatecnicas
        public async Task<IActionResult> Index(int id)
        {
            var fichaTecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.Diagnosticosolucions)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync(d => d.RecepcionEquipoId == id);

            if (fichaTecnica == null)
            {
                return RedirectToAction("Create", new { id });
            }

            return View(fichaTecnica);
        }

        //Listar Datos ficha Tecnica por Recepción de Equipos de Cliente: VerDatosFichaTecnicaPorRecepcion

        public async Task<IActionResult> VerDatosFichaTecnicaPorRecepcion(int id)
        {
            var mercydevsEjercicio2Context = _context.Datosfichatecnicas
                .Where(d => d.RecepcionEquipoId == id)
                .Include(d => d.RecepcionEquipo);
            ViewData["IdRecepcionEquipo"] = id;
            return View(await mercydevsEjercicio2Context.ToListAsync());
        }


        // GET: Datosfichatecnicas/Diagnosticosolucionpordatosficha/5
        public async Task<IActionResult> Diagnosticosolucionpordatosficha(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verdiagnostico = await _context.Datosfichatecnicas
                .Include(r => r.Diagnosticosolucions)
                .Include(r => r.RecepcionEquipo)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (verdiagnostico == null)
            {
                return NotFound();
            }

            return View(verdiagnostico);
        }

        // GET: Datosfichatecnicas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }

            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Create
        public IActionResult Create(int? id)
        {
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id");
            ViewData["IdRecepcionEquipo"] = id;
            return View();
        }

        // POST: Datosfichatecnicas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id,[Bind("IdDatosFichaTecnica,FechaInicio,FechaFinalizacion,PobservacionesRecomendaciones,Soinstalado,SuiteOfficeInstalada,LectorPdfinstalado,NavegadorWebInstalado,AntivirusInstalado,RecepcionEquipoId")] Datosfichatecnica datosfichatecnica)
        {
            if (datosfichatecnica.FechaInicio != null)
            {
                datosfichatecnica.RecepcionEquipoId = Convert.ToInt32(id);
                _context.Add(datosfichatecnica);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Recepcionequipoes");
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas.FindAsync(id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // POST: Datosfichatecnicas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDatosFichaTecnica,FechaInicio,FechaFinalizacion,PobservacionesRecomendaciones,Soinstalado,SuiteOfficeInstalada,LectorPdfinstalado,NavegadorWebInstalado,AntivirusInstalado,RecepcionEquipoId")] Datosfichatecnica datosfichatecnica)
        {
            if (id != datosfichatecnica.IdDatosFichaTecnica)
            {
                return NotFound();
            }

            if (datosfichatecnica.FechaInicio != null)
            {
                try
                {
                    _context.Update(datosfichatecnica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatosfichatecnicaExists(datosfichatecnica.IdDatosFichaTecnica))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Inicio));
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }

            return View(datosfichatecnica);
        }

        // POST: Datosfichatecnicas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var datosfichatecnica = await _context.Datosfichatecnicas.FindAsync(id);
            if (datosfichatecnica != null)
            {
                _context.Datosfichatecnicas.Remove(datosfichatecnica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DatosfichatecnicaExists(int id)
        {
            return _context.Datosfichatecnicas.Any(e => e.IdDatosFichaTecnica == id);
        }
    }
}

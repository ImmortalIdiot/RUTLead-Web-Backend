using api.Data;
using api.Dto;
using api.Helpers;
using api.Interfaces;
using api.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/RUTLead/student")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepo;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentRepository studentRepo, ILogger<StudentController> logger)
        {
            _studentRepo = studentRepo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject queryObject)
        {

            _logger.LogInformation("Get all students");

            var students = await _studentRepo.GetAllAsync(queryObject);

            var studentDto = students.Select(s => s.ToStudentDto());

            return Ok(students);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {

            _logger.LogInformation("Get student with {id} ID", id);
            var student = await _studentRepo.GetByIdAsync(id);

            if (student == null)
            { 
                _logger.LogWarning("Student with {id} ID doesn't exist", id);
                return NotFound();
            }
            _logger.LogInformation("Successful login for {id} ID", id);
            return Ok(student.ToStudentDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentRequestDto studentDto)
        {
            var studentModel = studentDto.ToStudentFromCreate();
            await _studentRepo.CreateAsync(studentModel);
            return CreatedAtAction(nameof(GetById), new {id = studentModel.StudentId}, studentModel.ToStudentDto());
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStudentRequestDto updateDto)
        {
            _logger.LogInformation("Update student {updateDto.StudentId}", updateDto.StudentId);
            var studentModel = await _studentRepo.UpdateAsync(id, updateDto);

            if (studentModel == null)
            {
                _logger.LogWarning("Fail to update, student {updateDto.StudentId} doesn't exist", updateDto.StudentId);
                return NotFound();
            }
            _logger.LogInformation("Successful update for {updateDto.StudentId}", updateDto.StudentId);
            return Ok(studentModel.ToStudentDto());
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            _logger.LogInformation("Delete student {id}", id);
            var studentModel = await _studentRepo.DeleteAsync(id);

            if (studentModel == null)
            {
                _logger.LogWarning("Delete fail, student {id} doesn't exist", id);
                return NotFound();
            }
            _logger.LogInformation("Remove student {id}", id);
            return NoContent();
        }
    }
}

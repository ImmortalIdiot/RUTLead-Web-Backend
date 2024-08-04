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
        private readonly ApiDBContext context;
        private readonly IStudentRepository studentRepo;
        private readonly ILogger<StudentController> logger;

        public StudentController(ApiDBContext context, IStudentRepository studentRepo, ILogger<StudentController> logger)
        {
            this.studentRepo = studentRepo;
            this.context = context;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject queryObject)
        {

            logger.LogInformation("Get all students");

            var students = await studentRepo.GetAllAsync(queryObject);

            var studentDto = students.Select(s => s.ToStudentDto());

            return Ok(students);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {

            logger.LogInformation($"Get student with {id}ID");
            var student = await studentRepo.GetByIdAsync(id);

            if (student == null)
            { 
                logger.LogWarning($"Student with {id}ID doesn't exist");
                return NotFound();
            }
            logger.LogInformation($"Successful login for {id}ID");
            return Ok(student.ToStudentDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStudentRequestDto studentDto)
        {
            var studentModel = studentDto.ToStudentFromCreate();
            await studentRepo.CreateAsync(studentModel);
            return CreatedAtAction(nameof(GetById), new {id = studentModel.StudentId}, studentModel.ToStudentDto());
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStudentRequestDto updateDto)
        {
            logger.LogInformation($"Update student {updateDto.StudentId}");
            var studentModel = await studentRepo.UpdateAsync(id, updateDto);

            if (studentModel == null)
            {
                logger.LogWarning($"Fail to update, student {updateDto.StudentId} doesn't exist");
                return NotFound();
            }
            logger.LogInformation($"Successful update for {updateDto.StudentId}");
            return Ok(studentModel.ToStudentDto());
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            logger.LogInformation($"Delete student {id}");
            var studentModel = await studentRepo.DeleteAsync(id);

            if (studentModel == null)
            {
                logger.LogWarning($"Delete fail, student {id} doesn't exist");
                return NotFound();
            }
            logger.LogInformation($"Remove student {id}");
            return NoContent();
        }
    }
}
